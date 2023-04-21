using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.FiveSevenPay.IntegrationTests;

internal static class FiveSevenPayTestHost
{
	public static WebApplicationFactory<Program> WebApplicationFactory => new WebApplicationFactory<Program>()
		.WithWebHostBuilder(hostBuilder => hostBuilder
			.ConfigureLogging((_, logging) => logging.ClearProviders())
			//.ConfigureAppConfiguration(configBuilder =>
			//{
			//	configBuilder.Sources.Clear();

			//	_ = configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
			//	{
			//		["FiveSevenPay:PaymentApiUrl"] = "http://php.578899.net",
			//		["FiveSevenPay:CallbackBaseUrl"] = "https://dev-payment.rpgp.cc/fivesevenpay-local",
			//		["FiveSevenPay:PaymentServiceBaseUrl"] = "http://localhost:5541"
			//	});
			//})
			.ConfigureServices(services => services
				.AddFakeNatsMessageQueue()));

}
