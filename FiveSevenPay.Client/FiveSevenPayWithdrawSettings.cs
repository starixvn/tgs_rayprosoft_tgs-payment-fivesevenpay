namespace Tgs.Payment.FiveSevenPay.Client;

public record FiveSevenPayWithdrawSettings(
	string MerchantKey,
	string PrivateKeyBase64,
	string PublicKeyBase64,
	string Currency,
	FiveSevenPaymentType PaymentType,
	Guid FormId,
	string BankCode,
	string BankCardName,
	string BankCardAccount);

	