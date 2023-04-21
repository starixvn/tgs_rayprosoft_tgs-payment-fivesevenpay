using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Payment.FiveSevenPay.Client.JsonConverters;

internal class FiveSevenPayTypeConverter : JsonConverter<FiveSevenPayType>
{
	public override FiveSevenPayType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var payType = reader.GetString();

		return FiveSevenPayType.Parse(payType!);
	}

	public override void Write(Utf8JsonWriter writer, FiveSevenPayType value, JsonSerializerOptions options)
		=> writer.WriteStringValue(value.Code);
}
