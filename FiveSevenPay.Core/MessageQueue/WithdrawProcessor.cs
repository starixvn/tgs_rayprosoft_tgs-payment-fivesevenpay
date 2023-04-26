using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Tgs.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.FiveSevenPay.Client;
using Tgs.Payment.Protos;

namespace Tgs.Payment.FiveSevenPay.MessageQueue;

internal class WithdrawProcessor : IMessageProcessor
{
	private readonly FiveSevenPayClient _fiveSevenPayClient;
	private readonly IPaymentClient _paymentClient;
	private readonly FiveSevenPayOptions _options;

	public WithdrawProcessor(
		FiveSevenPayClient fiveSevenPayClient,
		IPaymentClient paymentClient,
		IOptions<FiveSevenPayOptions> optionsAccessor)
	{
		_fiveSevenPayClient = fiveSevenPayClient ?? throw new ArgumentNullException(nameof(fiveSevenPayClient));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		_options = optionsAccessor.Value;
	}

	public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(
		ReadOnlyMemory<byte> data, 
		CancellationToken cancellationToken = default)
	{
		var request = WithdrawRequest.Parser.ParseFrom(data.Span);

		var settings = JsonSerializer.Deserialize<FiveSevenPayWithdrawSettings>(
			request.ApplyInfo,
			FiveSevenPayUtility.SerializerOptions)!;

		var order = new FiveSevenPayWithdrawOrder(
			settings.PayType,
			settings.Currency,
			decimal.Parse(request.Amount),
			settings.FormId,
			settings.BankCardAccount,
			new Uri($"{_options.CallbackBaseUrl}/withdraw/callback"));

		var privateKey = Encoding.UTF8.GetString(Convert.FromBase64String(settings.PrivateKey));

		var withdrawResponse = await _fiveSevenPayClient.WithdrawAsync(
			settings.MerchantKey,
			privateKey,
			order,
			cancellationToken).ConfigureAwait(false);

		var applyInfo = new FiveSevenPayWithdrawApplyInfo(
			settings,
			new FiveSevenPayWithdrawResponse(
				withdrawResponse.FormId));

		await _paymentClient.SaveWithdrawFormThirdPartyOrderNoAsync(
			settings.FormId,
			withdrawResponse.FormId.ToString(),
			cancellationToken);

		var msg = new WithdrawResponse
		{
			Result = JsonSerializer.Serialize(applyInfo)
		};

		return msg.ToByteArray();
	}
}
