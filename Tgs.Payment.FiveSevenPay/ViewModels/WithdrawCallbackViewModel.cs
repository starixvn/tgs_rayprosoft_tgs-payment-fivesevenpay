using System.Text.Json.Serialization;

namespace Tgs.Payment.FiveSevenPay.ViewModels;

public class WithdrawCallbackViewModel
{
	[JsonPropertyName("outID")]
	public Guid FormId { get; init; }

	public string Status { get; init; } = default!;

	public string? Currency { get; init; }

	public decimal Amount { get; init; }

	[JsonPropertyName("oamount")]
	public decimal? OriginalAmount { get; init; }

	public decimal? AgreeAmount { get; init; }

	public string OrderType { get; init; } = default!;

	public string? Remark { get; init; }
}
