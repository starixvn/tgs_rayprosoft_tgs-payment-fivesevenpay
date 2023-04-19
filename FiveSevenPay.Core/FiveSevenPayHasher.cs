using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.FiveSevenPay;

public class FiveSevenPayHasher
{
	private readonly ILogger<FiveSevenPayHasher> _logger;

	private JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

	public FiveSevenPayHasher(
		ILogger<FiveSevenPayHasher> logger)
	{
		_logger = logger;
	}

	public string ComputeHash(
		string privateKey,
		IEnumerable<KeyValuePair<string, string?>> dict,
		long timestamp)
	{
		var json = JsonSerializer.Serialize(
			dict.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
				.ToDictionary(p => p.Key,
					p => p.Value!.ToString()), 
			_jsonSerializerOptions);
		
		var fullText = $"{json}{timestamp}";

		_logger.LogInformation("FullText: {fullText}", fullText);

		var shaHashed = SHA256.HashData(Encoding.UTF8.GetBytes(fullText));

		var rsa = RSA.Create();
		rsa.ImportFromPem(privateKey);

		var signature = rsa.SignHash(shaHashed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

		var hash =  Convert.ToBase64String(signature);

		_logger.LogInformation("Hash: {hash}", hash);

		return hash;
	}
}