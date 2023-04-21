using Tgs.Payment.Clients;
using Tgs.Payment.FiveSevenPay.Client;

namespace Tgs.Payment.FiveSevenPay;

public record WithdrawForm(
	Guid Id,
	string? UserId,
	UserType UserType,
	Guid BrandId,
	Guid? BrandPaymentId,
	decimal Amount,
	PaymentFlowState Status,
	Guid BankInfoId,
	FiveSevenPayWithdrawApplyInfo? ApplyInfo,
	string? PaymentReturn = null);