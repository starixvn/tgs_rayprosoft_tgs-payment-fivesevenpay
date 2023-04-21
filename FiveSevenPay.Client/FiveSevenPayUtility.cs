using System.Text.Json;
using System.Text.Json.Serialization;
using Tgs.Payment.FiveSevenPay.Client.JsonConverters;

namespace Tgs.Payment.FiveSevenPay.Client;

public class FiveSevenPayUtility
{
	public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		Converters =
		{
			new FiveSevenPayTypeConverter()
		}
	};
}