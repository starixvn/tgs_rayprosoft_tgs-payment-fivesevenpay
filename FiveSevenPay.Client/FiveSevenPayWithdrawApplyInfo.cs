namespace Tgs.Payment.FiveSevenPay.Client;

public record FiveSevenPayWithdrawApplyInfo(
	FiveSevenPayWithdrawSettings Settings,
	FiveSevenPayWithdrawResponse Response);