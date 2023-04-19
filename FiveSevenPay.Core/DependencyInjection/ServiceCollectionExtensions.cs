// ReSharper disable CheckNamespace

using System.Reflection;
using Tgs.Payment.FiveSevenPay;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddFiveSevenPay(
		this IServiceCollection services,
		Action<FiveSevenPayOptions> configureOptions,
		Action<IHttpClientBuilder> configureFiveSevenPayClient)
	{
		configureFiveSevenPayClient?.Invoke(services.AddHttpClient<FiveSevenPayClient>());

		return services
			.AddOptions<FiveSevenPayOptions>()
			.Configure(configureOptions)
			.Services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddSingleton<FiveSevenPayHasher>();
	}
}
