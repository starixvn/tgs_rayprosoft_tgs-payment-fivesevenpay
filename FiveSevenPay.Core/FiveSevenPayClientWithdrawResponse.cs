using System.Text.Json.Serialization;

namespace Tgs.Payment.FiveSevenPay;

internal record FiveSevenPayClientWithdrawResponse(
	[property: JsonPropertyName("outID")] Guid FormId);
