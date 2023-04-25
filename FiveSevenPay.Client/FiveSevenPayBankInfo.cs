using System.Text.Json.Serialization;

namespace Tgs.Payment.FiveSevenPay.Client;

public record FiveSevenPayBankInfo(
	[property: JsonPropertyName("username")] string BankCardName,
	[property: JsonPropertyName("phone")] string BankCardAccount);

