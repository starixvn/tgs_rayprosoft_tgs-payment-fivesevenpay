using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Payment.FiveSevenPay;
using Tgs.Payment.FiveSevenPay.InputFormatters;
using Tgs.Payment.FiveSevenPay.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.AddJsonFile("/config/appsettings.json", true)
	.AddEnvironmentVariables();

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
			fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
				"MachineName",
				() => Environment.MachineName));
		});
	});

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

builder.Services.AddControllers(options 
	=> options.InputFormatters.Add(new TextPlainInputFormatter()));

builder.Services
	.AddProfileViewer()
	.AddHttpLogging(logging =>
	{
		logging.LoggingFields = HttpLoggingFields.All;
		logging.MediaTypeOptions.AddText("application/x-www-form-urlencoded");
		logging.MediaTypeOptions.AddText("application/json");
	})
	.AddOpenTelemetry()
	.ConfigureResource(_ => ResourceBuilder.CreateDefault().AddEnvironmentVariableDetector())
	.WithTracing(tracerProviderBuilder => tracerProviderBuilder
		.AddSource("Tgs.*")
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddProfileViewExporter()
		.AddOtlpExporter())
	.WithMetrics(meterProviderBuilder => meterProviderBuilder
		.AddRuntimeInstrumentation()
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddOtlpExporter())
	.Services
	.Configure<AspNetCoreInstrumentationOptions>(options => 
		options.Filter = httpContext 
			=> !httpContext.Request.Path.HasValue
			   || !httpContext.Request.Path.StartsWithSegments("/swagger")
			   && !httpContext.Request.Path.StartsWithSegments("/healthz")
			   && !httpContext.Request.Path.StartsWithSegments("/metrics")
			   && !new[] { "html", "ico", "png", "jpg" }.Any(
				   ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));

builder.Services
	//.AddEndpointsApiExplorer()
	.AddSwaggerGen(options =>
	{
		options.SwaggerDoc("Payment", new OpenApiInfo
		{
			Title = "FiveSevenPay Service",
			Version = "Payment"
		});

		options.SwaggerDoc("DataConverter", new OpenApiInfo
		{
			Title = "FiveSevenPay Service",
			Version = "DataConverter"
		});
		
		options.DocInclusionPredicate((groupName, desc)
			=> desc.GroupName == groupName);
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

app.MapSwagger();
app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/swagger/Payment/swagger.json", "Tgs FiveSevenPay Service - Payment");
	options.SwaggerEndpoint("/swagger/DataConverter/swagger.json", "Tgs FiveSevenPay Service - DataConverter");
});

app.Run();
