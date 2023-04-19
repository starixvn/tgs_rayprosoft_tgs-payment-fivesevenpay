namespace Tgs.Payment.FiveSevenPay;

public class FiveSevenPayOptions
{
	public required Uri PaymentApiUrl { get; set; }
	public required Uri CallbackBaseUrl { get; set; }
	public required Uri PaymentServiceBaseUrl { get; set; }
}
