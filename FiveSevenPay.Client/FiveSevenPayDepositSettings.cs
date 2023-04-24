using System.Text.Json.Serialization;
using Tgs.Payment.FiveSevenPay.Client.JsonConverters;

namespace Tgs.Payment.FiveSevenPay.Client;

public record FiveSevenPayDepositSettings(
	string MerchantKey,
	string PrivateKey,
	string PublicKey,
	string Currency,
	[property: JsonConverter(typeof(FiveSevenPayTypeConverter))] FiveSevenPayType PayType,
	Guid FormId);

	