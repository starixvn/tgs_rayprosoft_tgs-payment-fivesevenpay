namespace Tgs.Payment.FiveSevenPay.Client;

public record FiveSevenPayDepositResponse(
	Guid FormId,
	decimal Amount,
	decimal? RealAmount,
	string? Remark,
	Uri RedirectUrl);
