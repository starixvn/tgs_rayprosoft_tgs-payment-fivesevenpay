namespace Tgs.Payment.FiveSevenPay.Client;

public record FiveSevenPayWithdrawSettings(
	string MerchantKey,
	string PrivateKeyBase64,
	string PublicKeyBase64,
	string Currency,
	FiveSevenPayType PayType,
	Guid FormId,
	string BankCardName,
	string BankCardAccount);

	