using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Tgs.Payment.FiveSevenPay.Client;

namespace Tgs.Payment.FiveSevenPay;

internal class FiveSevenPayClient
{
	private readonly HttpClient _httpClient;
	private readonly FiveSevenPayHasher _fiveSevenPayHasher;
	private readonly ILogger<FiveSevenPayClient> _logger;

	private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

	private readonly IReadOnlyDictionary<FiveSevenPaymentType, string> _verificationTypeMap =
		new Dictionary<FiveSevenPaymentType, string>
		{
			[FiveSevenPaymentType.GCash] = "Amount"
		};

	public FiveSevenPayClient(
		HttpClient httpClient,
		FiveSevenPayHasher fiveSevenPayHasher,
		ILogger<FiveSevenPayClient> logger)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		_fiveSevenPayHasher = fiveSevenPayHasher ?? throw new ArgumentNullException(nameof(fiveSevenPayHasher));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask<FiveSevenPayClientDepositResponse> DepositAsync(
		string merchantKey, 
		string privateKey, 
		FiveSevenPayDepositOrder order, 
		CancellationToken cancellationToken = default)
	{
		if (!_verificationTypeMap.TryGetValue(order.PaymentType, out var verificationType))
			throw new ArgumentException("Invalid payment type");

		var data = new Dictionary<string, string?>
		{
			["merchantKey"] = merchantKey,
			["amount"] = order.Amount.ToString(),
			["currency"] = order.Currency,
			["outID"] = order.FormId.ToString(),
			["verificationType"] = verificationType,
			["payType"] = order.PaymentType.Code,
			["notifyUrl"] = order.CallbackUrl.AbsoluteUri
		};

		var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

		var signature = _fiveSevenPayHasher.ComputeHash(privateKey, data, timestamp);
		
		_httpClient.DefaultRequestHeaders.Add("key", merchantKey);
		_httpClient.DefaultRequestHeaders.Add("signature", signature);
		_httpClient.DefaultRequestHeaders.Add("timestamp", timestamp.ToString());

		var response = await _httpClient.PostAsJsonAsync(
			"/api?option=mutation&field=createTradeAPI",
			data,
			_jsonSerializerOptions,
			cancellationToken).ConfigureAwait(false);

		var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

		if (_logger.IsEnabled(LogLevel.Information))
			_logger.LogInformation("Deposit Body: {body}", body);

		_ = response.EnsureSuccessStatusCode();

		var node = JsonSerializer.Deserialize<JsonNode>(body, _jsonSerializerOptions);

		if (node is null)
			throw new ApiFailException("Deposit response can't read.");

		if (node["code"]!.ToString() != "0")
			throw new ApiFailException(node["msg"]?.GetValue<string>());

		return node["data"].Deserialize<FiveSevenPayClientDepositResponse>(_jsonSerializerOptions)!;
	}
}