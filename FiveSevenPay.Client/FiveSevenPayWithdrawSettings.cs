using System.Text.Json.Serialization;
using Tgs.Payment.FiveSevenPay.Client.JsonConverters;

namespace Tgs.Payment.FiveSevenPay.Client;

public record FiveSevenPayWithdrawSettings(
	string MerchantKey,
	string PrivateKeyBase64,
	string PublicKeyBase64,
	string Currency,
	[property: JsonConverter(typeof(FiveSevenPayTypeConverter))] FiveSevenPayType PayType,
	Guid FormId,
	string BankCardName,
	string BankCardAccount);

	