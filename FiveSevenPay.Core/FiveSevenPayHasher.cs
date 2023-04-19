using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.FiveSevenPay;

public class FiveSevenPayHasher
{
	private readonly ILogger<FiveSevenPayHasher> _logger;

	public FiveSevenPayHasher(
		ILogger<FiveSevenPayHasher> logger)
	{
		_logger = logger;
	}

	public string ComputeHash(
		string privateKeyBase64,
		IEnumerable<KeyValuePair<string, object?>> dict,
		long timestamp)
	{
		var json = JsonSerializer.Serialize(
			dict.Where(kvp => kvp.Value is not null && !string.IsNullOrEmpty(kvp.Value.ToString()))
				.ToDictionary(p => p.Key,
					p => WebUtility.UrlEncode(p.Value!.ToString())));
		
		var fullText = $"{json}{timestamp}";

		_logger.LogInformation("FullText: {fullText}", fullText);

		var shaHashed = SHA256.HashData(Encoding.UTF8.GetBytes(fullText));

		var data = Convert.FromBase64String(privateKeyBase64);
		var pem = Encoding.UTF8.GetString(data);

		var rsa = RSA.Create();
		rsa.ImportFromPem(pem);

		var signature = rsa.SignHash(shaHashed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		var hash =  Convert.ToBase64String(signature);

		_logger.LogInformation("Hash: {hash}", hash);

		return hash;
	}
}