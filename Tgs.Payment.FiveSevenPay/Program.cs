using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using Tgs.Payment.FiveSevenPay;
using Tgs.Payment.FiveSevenPay.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddFiveSevenPay(
		options => builder.Configuration.GetSection("FiveSevenPay").Bind(options),
		clientBuilder => clientBuilder
			.ConfigureHttpClient((sp, http) =>
			{
				var options = sp.GetRequiredService<IOptions<FiveSevenPayOptions>>().Value;

				http.BaseAddress = options.PaymentApiUrl;
			}).AddLoggingHandler<FiveSevenPayOptions>())
	.AddPaymentClient()
	.AddPaymentServiceClient(httpBuilder => httpBuilder
		.ConfigureHttpClient((sp, http) =>
		{
			var options = sp.GetRequiredService<IOptions<FiveSevenPayOptions>>().Value;

			http.BaseAddress = options.PaymentServiceBaseUrl;
		}));

builder.Services.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig => natsConfig
		.ConfigQueueOptions((options, _) => options.Url =
			builder.Configuration.GetConnectionString("Nats")!)
		.AddProcessor(
			ProcessorTypes.DepositProcessorType,
			"tgs.payment.fivesevenpay.deposit",
			"tgs.payment.fivesevenpay.deposit")
		.AddProcessor(
			ProcessorTypes.WithdrawProcessorType,
			"tgs.payment.fivesevenpay.withdraw",
			"tgs.payment.fivesevenpay.withdraw"))
	.AddNatsGlobPatternExchange("*");

builder.Services.AddControllers();

builder.Services
	.AddHttpLogging(logging =>
	{
		logging.LoggingFields = HttpLoggingFields.All;
		
		_ = logging.RequestHeaders.Add("signature");
		_ = logging.RequestHeaders.Add("timestamp");

		logging.MediaTypeOptions.AddText("application/x-www-form-urlencoded");
		logging.MediaTypeOptions.AddText("text/html");
		logging.MediaTypeOptions.AddText("application/json");
	});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHttpLogging();
app.MapControllers();
app.MapHealthChecks("/healthz");

app.Use(async (context, next) =>
{
	await next().ConfigureAwait(false);

	var logger = context.RequestServices.GetService<ILoggerFactory>()
		?.CreateLogger("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware");

	if (logger?.IsEnabled(LogLevel.Information) == true)
	{
		using var stream = new StreamReader(context.Request.Body);

		_ = await stream.ReadToEndAsync().ConfigureAwait(false);
	}
});

app.Run();
