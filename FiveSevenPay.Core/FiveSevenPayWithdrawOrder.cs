using System.Text.Json.Serialization;
using Tgs.Payment.FiveSevenPay.Client;

namespace Tgs.Payment.FiveSevenPay;

internal record FiveSevenPayWithdrawOrder(
	[property: JsonPropertyName("payment_type")] FiveSevenPaymentType PaymentType,
	string Currency,
	decimal Amount,
	[property: JsonPropertyName("form_id")] Guid FormId,
	[property: JsonPropertyName("bank_card_account")] string Phone,
	[property: JsonPropertyName("callback_url")] Uri CallbackUrl);