using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;

namespace Tgs.Payment.FiveSevenPay.IntegrationTests;

internal static class FiveSevenPayTestHost
{
	public static WebApplicationFactory<Program> WebApplicationFactory => new WebApplicationFactory<Program>()
		.WithWebHostBuilder(hostBuilder => hostBuilder
			.ConfigureLogging((_, logging) => logging.ClearProviders())
			.ConfigureServices(services => services
				.AddFakeNatsMessageQueue()
				.AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
					sp => new ConfigureNamedOptions<OtlpExporterOptions>(
						Options.DefaultName,
						exporterOptions => exporterOptions.TimeoutMilliseconds = 0))));
}
