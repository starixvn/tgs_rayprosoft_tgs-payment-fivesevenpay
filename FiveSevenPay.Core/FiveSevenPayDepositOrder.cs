using System.Text.Json.Serialization;
using Tgs.Payment.FiveSevenPay.Client;

namespace Tgs.Payment.FiveSevenPay;

internal record FiveSevenPayDepositOrder(
	[property: JsonPropertyName("payment_type")] FiveSevenPayType PayType,
	string Currency,
	decimal Amount,
	[property: JsonPropertyName("form_id")] Guid FormId,
	[property: JsonPropertyName("callback_url")] Uri CallbackUrl);