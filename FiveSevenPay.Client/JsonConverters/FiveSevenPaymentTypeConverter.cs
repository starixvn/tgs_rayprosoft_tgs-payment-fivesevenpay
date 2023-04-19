using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Payment.FiveSevenPay.Client.JsonConverters;

internal class FiveSevenPaymentTypeConverter : JsonConverter<FiveSevenPaymentType>
{
	public override FiveSevenPaymentType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var paymentType = reader.GetString();

		return FiveSevenPaymentType.Parse(paymentType!);
	}

	public override void Write(Utf8JsonWriter writer, FiveSevenPaymentType value, JsonSerializerOptions options)
		=> writer.WriteStringValue(value.Code);
}
