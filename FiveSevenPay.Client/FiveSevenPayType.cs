namespace Tgs.Payment.FiveSevenPay.Client;

public class FiveSevenPayType
{
	public static readonly FiveSevenPayType GCash = new("Gcash");

	private static readonly IReadOnlyDictionary<string, FiveSevenPayType> _PayTypes = 
		new Dictionary<string, FiveSevenPayType>(StringComparer.OrdinalIgnoreCase)
		{
			["gcash"] = GCash
		};

	public string Code { get; }

	private FiveSevenPayType(string code)
	{
		Code = code;
	}

	public static FiveSevenPayType? Parse(string paymentType)
		=> _PayTypes.TryGetValue(paymentType, out var value)
			? value
			: null;
}
