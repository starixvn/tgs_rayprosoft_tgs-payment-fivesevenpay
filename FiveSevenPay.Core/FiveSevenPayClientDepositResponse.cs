using System.Text.Json.Serialization;

namespace Tgs.Payment.FiveSevenPay;

internal record FiveSevenPayClientDepositResponse(
	[property: JsonPropertyName("outid")] Guid FormId,
	decimal Amount,
	decimal? RealAmount,
	[property: JsonPropertyName("payRemark")] string? Remark,
	[property: JsonPropertyName("redirecturl")] Uri RedirectUrl);
