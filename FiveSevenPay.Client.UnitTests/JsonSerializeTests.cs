using System.Text.Json;
using Tgs.Payment.FiveSevenPay.Client;

namespace FiveSevenPay.Client.UnitTests;

public class JsonSerializeTests
{
	[Fact]
	public void 序列化儲值設定()
	{
		// Arrange
		var sut = new FiveSevenPayDepositSettings(
			"test123",
			"1234567890",
			"1234567890",
			"PHP",
			FiveSevenPayType.GCash,
			Guid.Parse("fb6a7c26-6438-4508-9440-e3f07b91dcfa"));

		var expected = JsonSerializer.Serialize(JsonDocument.Parse("""
			{
				"merchantKey":"test123",
				"privateKey":"1234567890",
				"publicKey":"1234567890",
				"currency":"PHP",
				"payType":"Gcash",
				"formId":"fb6a7c26-6438-4508-9440-e3f07b91dcfa"
			}
			"""));

		// Act
		var actual = JsonSerializer.Serialize(sut, FiveSevenPayUtility.SerializerOptions);

		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void 反序列化儲值設定()
	{
		// Arrange
		var sut = """
			{
				"merchantKey":"test123",
				"privateKey":"1234567890",
				"publicKey":"1234567890",
				"currency":"PHP",
				"payType":"Gcash",
				"formId":"fb6a7c26-6438-4508-9440-e3f07b91dcfa"
			}
			""";

		var expected = new FiveSevenPayDepositSettings(
			"test123",
			"1234567890",
			"1234567890",
			"PHP",
			FiveSevenPayType.GCash,
			Guid.Parse("fb6a7c26-6438-4508-9440-e3f07b91dcfa"));

		// Act
		var actual = JsonSerializer.Deserialize<FiveSevenPayDepositSettings>(sut, FiveSevenPayUtility.SerializerOptions);

		// Assert
		Assert.Equal(expected, actual);
	}
}