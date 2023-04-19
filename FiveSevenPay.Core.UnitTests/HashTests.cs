using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
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
	public void HashTest()
	{
		var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

		var privateKey = @"
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
-----END RSA PRIVATE KEY-----";

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
		
		var publicKey = @"
-----BEGIN PUBLIC KEY-----
MIGeMA0GCSqGSIb3DQEBAQUAA4GMADCBiAKBgFjWApUuZQn9X3habxD8aFyFIHhR
3JGCzQUgUlqNMG31SkwlIlJ/akJ4vz3qBBafNxe4tyO4uqdxTzypUI9PXnCrk5z1
N+FMbaWX/8cr+QdnArkcsxJ0p+2Qy9rZSkacyCPjME9XJzIebKnf0XTGpT/z6T0c
yRI2HxD1sjHbvBZHAgMBAAE=
-----END PUBLIC KEY-----";

		var signature = Convert.FromBase64String(actual);

		var json = JsonSerializer.Serialize(
			dict.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
				.ToDictionary(p => p.Key,
					p => p.Value!.ToString()),
			jsonSerializerOptions);

		var fullText = $"{json}{timestamp}";
		
		var hashedBody = SHA256.HashData(Encoding.UTF8.GetBytes(fullText));

		var rsa = RSA.Create();
		rsa.ImportFromPem(publicKey);

		var result = rsa.VerifyHash(hashedBody, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

		Assert.True(result);
	}

	//[Fact]
	public async Task Test_Deposit()
	{
		var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

		var privateKey = @"
-----BEGIN RSA PRIVATE KEY-----
MIICWwIBAAKBgQC7qh3f+UFIEdbP8voKBiwiXx3MRkNnp8xcWqQ0CowXBaCaWtAn
/Xasf5dKMWUt6ihcK7Sm2h63wTUCrAZiJiZRHV52sVvsbvM6+3eBAh+jZu/P4lhk
+ZJOekz4QNQicNsuweH8cyThHmiBchA3bszCD3s8BI5OJ41MXWqp47H3MwIDAQAB
AoGANWvX1Uo/KyTo4YlpZDEdnys0cFIQHhHGOwQSej4GYwdXEJmJZ9jfT3uCtbwZ
8VgW5SYL5vNwix6U2I3fw1DY1wGVWPxUAtuGHW/BjNoncVqau24nVMKDpUsiNoAT
LLRJzcXRstEdJ/mbKSQSsYvTMhQPRQCPIx7KgPJ3eEy0JwECQQDXFyOL4OKur0eM
vwihG9w2M1biuOIRZAfd6jN2em7pQDZg5xJRLHnYH2qf4IaTkxsh3xxtCk6fEMB0
CjKapVndAkEA31uZKUEWRaGT2I89mPEteXZaI5eRQy4rLvHlQMbdJH9HNDLISGSE
0lFk/id9gEPHwv91AC+jkYVVEI+97KpsTwJACW3ZQBOOvGuwrkndnfpzr+iWQYAr
YOnKsn7G3MXHQPBwsdTPZ8GUYdYWvYWBQMztHpICVAFkqvJ4DV45E7B1/QJACNc1
Tr62xPbKRU4WIBGbvS0EEFiVdVVEfE0yx8eXRTSMevJvnv3ri++Q0cZ6XzNPmtZF
qUY5cS8X2Nn3qIBuXQJAANoUTkL8nejtdXoNFTvWNheRP8J10d1g3wNkvi0Qe+OK
jNcFxveAuXEHUBab7hrUfQtlthiYME9l/AP3PkyvkA==
-----END RSA PRIVATE KEY-----
	";

		var merchantKey = "2b223bce3f024a3f885d213770a0fc5e";

		var dict = new Dictionary<string, string?>
		{
			["merchantKey"] = merchantKey,
			["amount"] = "100",
			["currency"] = "PHP",
			//["outID"] = "b5c88f52-85f0-428c-a914-3124b0e4feb4",
			["outID"] = Guid.NewGuid().ToString(),
			["verificationType"] = "Amount",
			["payType"] = "Gcash",
			["remark"] = "",
			//["notifyUrl"] = HttpUtility.UrlEncode("https://dev-payment.rpgp.cc/fivesevenpay-local/deposit/callback"),
			["notifyUrl"] = "https://dev-payment.rpgp.cc/fivesevenpay-local/deposit/callback"
		};
		
		dict = dict.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
			.ToDictionary(p => p.Key, p => p.Value);

		var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		
		var json = JsonSerializer.Serialize(dict, jsonSerializerOptions);

		var fullText = $"{json}{timestamp}";

		_testOutputHelper.WriteLine("FullText: {0}", fullText);

		var shaHashed = SHA256.HashData(Encoding.UTF8.GetBytes(fullText));

		var rsa = RSA.Create();
		rsa.ImportFromPem(privateKey);
		
		var hash = rsa.SignHash(shaHashed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		
		var signature = Convert.ToBase64String(hash);

		_testOutputHelper.WriteLine("Signature: {0}", signature);


		using var httpClient = new HttpClient();
		

		httpClient.DefaultRequestHeaders.Add("key", merchantKey);
		httpClient.DefaultRequestHeaders.Add("signature", signature);
		httpClient.DefaultRequestHeaders.Add("timestamp", timestamp.ToString());

		var response = await httpClient.PostAsJsonAsync(
			"http://php.578899.net/api?option=mutation&field=createTradeAPI",
			dict,
			jsonSerializerOptions,
			CancellationToken.None);

		var res = await response.Content.ReadAsStringAsync(CancellationToken.None);

		_testOutputHelper.WriteLine("res: {0}", res);
	}

}
