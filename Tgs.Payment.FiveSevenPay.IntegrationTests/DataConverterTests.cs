using System.Net.Http.Json;
using System.Text;
using Tgs.Payment.FiveSevenPay.ViewModels;

namespace Tgs.Payment.FiveSevenPay.IntegrationTests;

public class DataConverterTests
{
	[Fact]
	public async Task 將Base64字串轉換至原始字串()
	{
		await using var web = FiveSevenPayTestHost.WebApplicationFactory;

		var httpClient = web.CreateClient();

		var base64 = "LS0tLS1CRUdJTiBSU0EgUFJJVkFURSBLRVktLS0tLQ0KTUlJQ1d3SUJBQUtCZ0ZqV0FwVXVaUW45WDNoYWJ4RDhhRnlGSUhoUjNKR0N6UVVnVWxxTk1HMzFTa3dsSWxKLw0KYWtKNHZ6M3FCQmFmTnhlNHR5TzR1cWR4VHp5cFVJOVBYbkNyazV6MU4rRk1iYVdYLzhjcitRZG5Bcmtjc3hKMA0KcCsyUXk5clpTa2FjeUNQak1FOVhKekllYktuZjBYVEdwVC96NlQwY3lSSTJIeEQxc2pIYnZCWkhBZ01CQUFFQw0KZ1lBNnlkV0h0OTR4a0RWZnhpaitDcXRkbnRQd0MreDFkeVFLSW11ZGRSWDVqbndPaS94OHI4azRHQVFuQ3ZpTg0KcCt1ampSL1ZTZWhyNnJaUEpKVWxZWmJUSFV0aGlKOVBlSWZoMytYalNYVmxqYWR6U2NOUzYrVnRpd3JVNm96dQ0KVGVBUHpkRVprVXBBQW5KM2wrUU13emJPRTZWMmdkdGZPU0trdGpGRVkxbkNzUUpCQUozR2R5Nm5JMnovdzVsZQ0KcUsySGRUTmNxKzA4WnRoTlZrdit6Mk91QTNkNnd1S1ZLSUljN1JyUHBwYXkyWEtYM1FQaHZSU1RCaTNtdFhFSw0Kak1RNnlsMENRUUNRSkUxSnpRNE9VdmJQRVJ3aS9yamt4VmVRWk9BbVBTUEdGMmc5M2NEcVp5bnVVRzFhOWl0TQ0KUDhZSmdKeDlWcHVUeGRselBWTVY5bURIU040a3hnRHpBa0IzcUpXclpFZVIyMGhWZ1YwY2ZxZDhsTVF2NEtkLw0KVk9nbUt0TFBIcUt5Y3dCcktBVFk2V2RKOWtLNStGckNkanJCUHV0UU4xRndicEVkY29yZ0tGWmhBa0E5Um5Gaw0Kc2pzTGM5RmY3VkljdmxRelRJRVE3UkVzc0VrSTZPSnppbFY4VUNiUEhIbWdkWTJxZUdIam9Eem1PMjhmdnJmVg0KK1hmY21rUkI4NFFiOWVCcEFrRUFqRUZrYkcxWEhoTTdmbTBOU3FLWVNaOFZYVExxeit3L0d1dXhqVnFOYnZYcw0Kb2p5QUhXV2VvWThKZi93WFM4TlVqVjFYZ1Q0TVU1WE5QM0M4VVcxeHdBPT0NCi0tLS0tRU5EIFJTQSBQUklWQVRFIEtFWS0tLS0t";

		var expected = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

		var viewModel = new FromBase64ViewModel
		{
			Base64String = base64
		};

		var response = await httpClient.PostAsJsonAsync("/dataconverter/base64:from", viewModel).ConfigureAwait(false);
		var actual = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task 將字串轉換至Base64字串()
	{
		await using var web = FiveSevenPayTestHost.WebApplicationFactory;

		var httpClient = web.CreateClient();

		var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "rsa_test.txt");

		var value = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

		var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

		using var content = new StringContent(value, Encoding.UTF8);
		
		using var form = new MultipartFormDataContent();
		form.Add(content, "file", "rsa_test.txt");

		var response = await httpClient.PostAsync("/dataconverter/base64:to", form).ConfigureAwait(false);
		var actual = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

		Assert.Equal(expected, actual);
	}
}
