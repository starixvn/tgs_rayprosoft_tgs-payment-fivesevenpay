namespace Tgs.Payment.FiveSevenPay.Client;

public record FiveSevenPayDepositSettings(
	string MerchantKey,
	string PrivateKeyBase64,
	string PublicKeyBase64,
	string Currency,
	FiveSevenPayType PayType,
	Guid FormId);

	