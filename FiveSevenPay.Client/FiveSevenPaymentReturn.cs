namespace Tgs.Payment.FiveSevenPay.Client;

public record FiveSevenPaymentReturn(
	Guid FormId,
	string? Currency,
	decimal Amount,
	decimal? OriginalAmount,
	decimal? AgreeAmount,
	string Status,
	string OrderType,
	string? Remark);