using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.Payment.FiveSevenPay.Client;
using Xunit.Abstractions;

namespace Tgs.Payment.FiveSevenPay.IntegrationTests;

public class DepositTests
{
	private readonly ITestOutputHelper _testOutputHelper;

	public DepositTests(
		ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
	}

	[Fact]
	public async Task 儲值回調呼叫()
	{
		var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
		var fakePaymentClient = Substitute.For<IPaymentClient>();

		await using var web = FiveSevenPayTestHost.WebApplicationFactory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
				.ConfigureServices(services => services
					.AddSingleton(fakePaymentServiceClient)
					.AddSingleton(fakePaymentClient)));
		
		var httpClient = web.CreateClient();

		var formId = Guid.Parse("a2554936-f976-4f60-b99a-f1c42f0b07f9");

		var settings = new FiveSevenPayDepositSettings(
			MerchantKey: "2b223bce3f024a3f885d213770a0fc5e",
			PrivateKey: "LS0tLS1CRUdJTiBSU0EgUFJJVkFURSBLRVktLS0tLQ0KTUlJQ1d3SUJBQUtCZ1FDN3FoM2YrVUZJRWRiUDh2b0tCaXdpWHgzTVJrTm5wOHhjV3FRMENvd1hCYUNhV3RBbg0KL1hhc2Y1ZEtNV1V0NmloY0s3U20yaDYzd1RVQ3JBWmlKaVpSSFY1MnNWdnNidk02KzNlQkFoK2padS9QNGxoaw0KK1pKT2VrejRRTlFpY05zdXdlSDhjeVRoSG1pQmNoQTNic3pDRDNzOEJJNU9KNDFNWFdxcDQ3SDNNd0lEQVFBQg0KQW9HQU5XdlgxVW8vS3lUbzRZbHBaREVkbnlzMGNGSVFIaEhHT3dRU2VqNEdZd2RYRUptSlo5amZUM3VDdGJ3Wg0KOFZnVzVTWUw1dk53aXg2VTJJM2Z3MURZMXdHVldQeFVBdHVHSFcvQmpOb25jVnFhdTI0blZNS0RwVXNpTm9BVA0KTExSSnpjWFJzdEVkSi9tYktTUVNzWXZUTWhRUFJRQ1BJeDdLZ1BKM2VFeTBKd0VDUVFEWEZ5T0w0T0t1cjBlTQ0KdndpaEc5dzJNMWJpdU9JUlpBZmQ2ak4yZW03cFFEWmc1eEpSTEhuWUgycWY0SWFUa3hzaDN4eHRDazZmRU1CMA0KQ2pLYXBWbmRBa0VBMzF1WktVRVdSYUdUMkk4OW1QRXRlWFphSTVlUlF5NHJMdkhsUU1iZEpIOUhORExJU0dTRQ0KMGxGay9pZDlnRVBId3Y5MUFDK2prWVZWRUkrOTdLcHNUd0pBQ1czWlFCT092R3V3cmtuZG5mcHpyK2lXUVlBcg0KWU9uS3NuN0czTVhIUVBCd3NkVFBaOEdVWWRZV3ZZV0JRTXp0SHBJQ1ZBRmtxdko0RFY0NUU3QjEvUUpBQ05jMQ0KVHI2MnhQYktSVTRXSUJHYnZTMEVFRmlWZFZWRWZFMHl4OGVYUlRTTWV2SnZudjNyaSsrUTBjWjZYek5QbXRaRg0KcVVZNWNTOFgyTm4zcUlCdVhRSkFBTm9VVGtMOG5lanRkWG9ORlR2V05oZVJQOEoxMGQxZzN3Tmt2aTBRZStPSw0Kak5jRnh2ZUF1WEVIVUJhYjdoclVmUXRsdGhpWU1FOWwvQVAzUGt5dmtBPT0NCi0tLS0tRU5EIFJTQSBQUklWQVRFIEtFWS0tLS0t",
			PublicKey: "LS0tLS1CRUdJTiBSU0EgUFVCTElDIEtFWS0tLS0tDQpNSUdKQW9HQkFOZ3JqSHVnVkhNcFJKRWV4OU1EcVR1UUdBSUZjS3VwUXNCVzJvZ1F5NktTRzRUN0JPaFc2ak9kDQowTk1CdnNNUWc1Yk1QTGFCZW5PQlBTN1dwS1VHVmtaSzBmL3V0RDUvRkRhNkdlaTVMb2tCbjFuM0hkMXp1d29JDQpENEtwemxuYXVydGxZMEw4WWo0UkRZdTlzWXhzY1JZNVh1WXBTS28xZElTRlFRMmZuUTczQWdNQkFBRT0NCi0tLS0tRU5EIFJTQSBQVUJMSUMgS0VZLS0tLS0=",
			"PHP",
			FiveSevenPayType.GCash,
			formId);

		var depositResponse = new FiveSevenPayDepositResponse(
			formId,
			100M,
			null,
			null,
			new Uri("http://localhost"));

		var applyInfo = new FiveSevenPayDepositApplyInfo(settings, depositResponse);

		_ = fakePaymentServiceClient.GetDepositFormAsync(
				Arg.Is(formId), 
				Arg.Any<CancellationToken>())
			.Returns(new DepositFormInfo
			{
				Id = formId,
				BrandId = Guid.NewGuid(),
				Status = PaymentFlowState.WaitPayment,
				ApplyInfo = JsonSerializer.Serialize(applyInfo, FiveSevenPayUtility.SerializerOptions)
			});

		var signature = "axgmlarWdjP261fW0xCmf65fmqqe1Vtntyv4W6EIH8YZOqUOPEosE72FPXlEV/2qUDAx5mjbfG6cAS1yLYM7MrMTwHIZVywLlfnDdLfaaIKaQ2RPH634XJlrn7ebFTg3IARC4gQunhaFcPDfDyF0WgiCx3eEryXD3w5iHB5qQtU=";

		var timestamp = 1681969474;
		
		var json = """
{"outID":"a2554936-f976-4f60-b99a-f1c42f0b07f9","status":"Payed","currency":"PHP","amount":"100","oamount":"100","agreeAmount":"100","orderType":"Trade"}		
""";

		httpClient.DefaultRequestHeaders.Add("signature", signature);
		httpClient.DefaultRequestHeaders.Add("timestamp", timestamp.ToString());

		var response = await httpClient.PostAsJsonAsync("/deposit/callback", JsonNode.Parse(json));

		var actual = await response.Content.ReadAsStringAsync();

		Assert.Equal("success", actual);

		_ = fakePaymentServiceClient.Received(1)
			.SubmitDepositFormStatusAsync(
				Arg.Is(formId),
				paymentReturn: Arg.Any<string>(),
				cancellationToken: Arg.Any<CancellationToken>());

		_ = fakePaymentClient.Received(1)
			.ApproveDepositAsync(
				Arg.Is(formId),
				Arg.Any<Guid>(),
				Arg.Is(applyInfo.Response.Amount),
				Arg.Any<CancellationToken>());
	}
}