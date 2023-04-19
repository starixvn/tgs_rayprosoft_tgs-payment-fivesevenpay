using Tgs.Payment.Clients;
using Tgs.Payment.FiveSevenPay.Client;

namespace Tgs.Payment.FiveSevenPay;

public record DepositForm(
	Guid Id,
	string PlayerCode,
	Guid BrandId,
	Guid BrandPaymentId,
	decimal Amount,
	PaymentFlowState Status,
	FiveSevenPayDepositApplyInfo? ApplyInfo,
	string? PaymentReturn = null);