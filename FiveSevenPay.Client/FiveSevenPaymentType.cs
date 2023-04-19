namespace Tgs.Payment.FiveSevenPay.Client;

public class FiveSevenPaymentType
{
	public static readonly FiveSevenPaymentType GCash = new("Gcash");

	private static readonly IReadOnlyDictionary<string, FiveSevenPaymentType> _PaymentTypes = new Dictionary<string, FiveSevenPaymentType>(StringComparer.InvariantCultureIgnoreCase)
	{
		["gcash"] = GCash
	};

	public string Code { get; }

	private FiveSevenPaymentType(string code)
	{
		Code = code;
	}

	public static FiveSevenPaymentType? Parse(string paymentType)
		=> _PaymentTypes.TryGetValue(paymentType, out var value)
			? value
			: null;
}
