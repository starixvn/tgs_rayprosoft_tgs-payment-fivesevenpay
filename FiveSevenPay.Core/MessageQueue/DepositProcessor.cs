using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Tgs.MessageQueue;
using Tgs.Payment.FiveSevenPay.Client;
using Tgs.Payment.Protos;

namespace Tgs.Payment.FiveSevenPay.MessageQueue;

internal class DepositProcessor:IMessageProcessor
{
	private readonly FiveSevenPayClient _fiveSevenPayClient;
	private readonly FiveSevenPayOptions _options;

	public DepositProcessor(
		FiveSevenPayClient fiveSevenPayClient,
		IOptions<FiveSevenPayOptions> optionsAccessor)
	{
		_fiveSevenPayClient = fiveSevenPayClient ?? throw new ArgumentNullException(nameof(fiveSevenPayClient));
		_options = optionsAccessor.Value;
	}

	public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(
		ReadOnlyMemory<byte> data, 
		CancellationToken cancellationToken = default)
	{
		var request = DepositRequest.Parser.ParseFrom(data.Span);

		var settings = JsonSerializer.Deserialize<FiveSevenPayDepositSettings>(
			request.ApplyInfo,
			FiveSevenPayUtility.SerializerOptions)!;

		var order = new FiveSevenPayDepositOrder(
			settings.PayType,
			settings.Currency,
			decimal.Parse(request.Amount),
			settings.FormId,
			new Uri($"{_options.CallbackBaseUrl}/deposit/callback"));

		var privateKey = Encoding.UTF8.GetString(Convert.FromBase64String(settings.PrivateKeyBase64));

		var depositResponse = await _fiveSevenPayClient.DepositAsync(
			settings.MerchantKey,
			privateKey,
			order,
			cancellationToken).ConfigureAwait(false);

		var applyInfo = new FiveSevenPayDepositApplyInfo(
			settings,
			new FiveSevenPayDepositResponse(
				depositResponse.FormId,
				depositResponse.Amount,
				depositResponse.RealAmount,
				depositResponse.Remark,
				depositResponse.RedirectUrl));

		var msg = new DepositResponse
		{
			Result = JsonSerializer.Serialize(applyInfo)
		};

		return msg.ToByteArray();
	}
}