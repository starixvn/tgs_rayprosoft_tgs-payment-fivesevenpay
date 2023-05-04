using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Tgs.Payment.FiveSevenPay;
using Xunit.Abstractions;

namespace FiveSevenPay.Core.UnitTests;

public class HashTests
{
	private readonly ITestOutputHelper _testOutputHelper;

	public HashTests(
		ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
	}

	[Fact]
	public void Compute_Hash_Test()
	{
		var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

		var privateKey = """
			-----BEGIN RSA PRIVATE KEY-----
			MIICWwIBAAKBgFjWApUuZQn9X3habxD8aFyFIHhR3JGCzQUgUlqNMG31SkwlIlJ/
			akJ4vz3qBBafNxe4tyO4uqdxTzypUI9PXnCrk5z1N+FMbaWX/8cr+QdnArkcsxJ0
			p+2Qy9rZSkacyCPjME9XJzIebKnf0XTGpT/z6T0cyRI2HxD1sjHbvBZHAgMBAAEC
			gYA6ydWHt94xkDVfxij+CqtdntPwC+x1dyQKImuddRX5jnwOi/x8r8k4GAQnCviN
			p+ujjR/VSehr6rZPJJUlYZbTHUthiJ9PeIfh3+XjSXVljadzScNS6+VtiwrU6ozu
			TeAPzdEZkUpAAnJ3l+QMwzbOE6V2gdtfOSKktjFEY1nCsQJBAJ3Gdy6nI2z/w5le
			qK2HdTNcq+08ZthNVkv+z2OuA3d6wuKVKIIc7RrPppay2XKX3QPhvRSTBi3mtXEK
			jMQ6yl0CQQCQJE1JzQ4OUvbPERwi/rjkxVeQZOAmPSPGF2g93cDqZynuUG1a9itM
			P8YJgJx9VpuTxdlzPVMV9mDHSN4kxgDzAkB3qJWrZEeR20hVgV0cfqd8lMQv4Kd/
			VOgmKtLPHqKycwBrKATY6WdJ9kK5+FrCdjrBPutQN1FwbpEdcorgKFZhAkA9RnFk
			sjsLc9Ff7VIcvlQzTIEQ7REssEkI6OJzilV8UCbPHHmgdY2qeGHjoDzmO28fvrfV
			+XfcmkRB84Qb9eBpAkEAjEFkbG1XHhM7fm0NSqKYSZ8VXTLqz+w/GuuxjVqNbvXs
			ojyAHWWeoY8Jf/wXS8NUjV1XgT4MU5XNP3C8UW1xwA==
			-----END RSA PRIVATE KEY-----
			""";

		var merchantKey = "2b223bce3f024a3f885d213770a0fc5e";

		var dict = new Dictionary<string, string?>
		{
			["merchantKey"] = merchantKey,
			["amount"] = "100",
			["currency"] = "PHP",
			["outID"] = "b5c88f52-85f0-428c-a914-3124b0e4feb4",
			["verificationType"] = "Amount",
			["payType"] = "Gcash",
			["remark"] = "",
			["notifyUrl"] = "http://localhost/deposit/callback"
		};

		var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

		var sut = new FiveSevenPayHasher(NullLogger<FiveSevenPayHasher>.Instance);

		var actual = sut.ComputeHash(privateKey, dict, timestamp);

		var publicKey = """
			-----BEGIN PUBLIC KEY-----
			MIGeMA0GCSqGSIb3DQEBAQUAA4GMADCBiAKBgFjWApUuZQn9X3habxD8aFyFIHhR
			3JGCzQUgUlqNMG31SkwlIlJ/akJ4vz3qBBafNxe4tyO4uqdxTzypUI9PXnCrk5z1
			N+FMbaWX/8cr+QdnArkcsxJ0p+2Qy9rZSkacyCPjME9XJzIebKnf0XTGpT/z6T0c
			yRI2HxD1sjHbvBZHAgMBAAE=
			-----END PUBLIC KEY-----
			""";

		var signature = Convert.FromBase64String(actual);

		var json = JsonSerializer.Serialize(
			dict.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
				.ToDictionary(p => p.Key,
					p => p.Value!.ToString()),
			jsonSerializerOptions);

		var fullText = $"{json}{timestamp}";

		_testOutputHelper.WriteLine("full text: {0}", fullText);

		var hashedBody = SHA256.HashData(Encoding.UTF8.GetBytes(fullText));

		var rsa = RSA.Create();
		rsa.ImportFromPem(publicKey);

		var result = rsa.VerifyHash(hashedBody, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

		Assert.True(result);
	}

	[Fact]
	public void Verify_Hash_Test()
	{
		var privateKey = """
			-----BEGIN RSA PRIVATE KEY-----
			MIICWwIBAAKBgFjWApUuZQn9X3habxD8aFyFIHhR3JGCzQUgUlqNMG31SkwlIlJ/
			akJ4vz3qBBafNxe4tyO4uqdxTzypUI9PXnCrk5z1N+FMbaWX/8cr+QdnArkcsxJ0
			p+2Qy9rZSkacyCPjME9XJzIebKnf0XTGpT/z6T0cyRI2HxD1sjHbvBZHAgMBAAEC
			gYA6ydWHt94xkDVfxij+CqtdntPwC+x1dyQKImuddRX5jnwOi/x8r8k4GAQnCviN
			p+ujjR/VSehr6rZPJJUlYZbTHUthiJ9PeIfh3+XjSXVljadzScNS6+VtiwrU6ozu
			TeAPzdEZkUpAAnJ3l+QMwzbOE6V2gdtfOSKktjFEY1nCsQJBAJ3Gdy6nI2z/w5le
			qK2HdTNcq+08ZthNVkv+z2OuA3d6wuKVKIIc7RrPppay2XKX3QPhvRSTBi3mtXEK
			jMQ6yl0CQQCQJE1JzQ4OUvbPERwi/rjkxVeQZOAmPSPGF2g93cDqZynuUG1a9itM
			P8YJgJx9VpuTxdlzPVMV9mDHSN4kxgDzAkB3qJWrZEeR20hVgV0cfqd8lMQv4Kd/
			VOgmKtLPHqKycwBrKATY6WdJ9kK5+FrCdjrBPutQN1FwbpEdcorgKFZhAkA9RnFk
			sjsLc9Ff7VIcvlQzTIEQ7REssEkI6OJzilV8UCbPHHmgdY2qeGHjoDzmO28fvrfV
			+XfcmkRB84Qb9eBpAkEAjEFkbG1XHhM7fm0NSqKYSZ8VXTLqz+w/GuuxjVqNbvXs
			ojyAHWWeoY8Jf/wXS8NUjV1XgT4MU5XNP3C8UW1xwA==
			-----END RSA PRIVATE KEY-----
			""";

		var publicKey = """
			-----BEGIN PUBLIC KEY-----
			MIGeMA0GCSqGSIb3DQEBAQUAA4GMADCBiAKBgFjWApUuZQn9X3habxD8aFyFIHhR
			3JGCzQUgUlqNMG31SkwlIlJ/akJ4vz3qBBafNxe4tyO4uqdxTzypUI9PXnCrk5z1
			N+FMbaWX/8cr+QdnArkcsxJ0p+2Qy9rZSkacyCPjME9XJzIebKnf0XTGpT/z6T0c
			yRI2HxD1sjHbvBZHAgMBAAE=
			-----END PUBLIC KEY-----
			""";

		var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

		var json = """
			{
				"outID":"a2554936-f976-4f60-b99a-f1c42f0b07f9",
				"status":"Payed",
				"currency":"PHP",
				"amount":"100",
				"oamount":"100",
				"agreeAmount":"100",
				"orderType":"Trade"
			}
			""";

		var fullText = $"{json}{timestamp}";

		_testOutputHelper.WriteLine("full text: {0}", fullText);

		var hashedBody = SHA256.HashData(Encoding.UTF8.GetBytes(fullText));

		var rsa = RSA.Create();
		rsa.ImportFromPem(privateKey);

		var hash = rsa.SignHash(hashedBody, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

		var signature = Convert.ToBase64String(hash);

		var sut = new FiveSevenPayHasher(NullLogger<FiveSevenPayHasher>.Instance);

		var actual = sut.VerifyHash(publicKey, json, signature, timestamp);

		Assert.True(actual);
	}
}
