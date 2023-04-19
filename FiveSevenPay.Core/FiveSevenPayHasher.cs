using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.FiveSevenPay;

public class FiveSevenPayHasher
{
	private readonly ILogger<FiveSevenPayHasher> _logger;

	private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

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

		_logger.LogTrace("ComputeHash FullText: {fullText}", fullText);

		var shaHashed = SHA256.HashData(Encoding.UTF8.GetBytes(fullText));

		var rsa = RSA.Create();
		rsa.ImportFromPem(privateKey);

		var signature = rsa.SignHash(shaHashed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

		var hash =  Convert.ToBase64String(signature);

		_logger.LogTrace("Hash: {hash}", hash);

		return hash;
	}

	public bool VerifyHash(
		string publicKey, 
		string requestData, 
		string signature, 
		long timestamp)
	{
		var fullText = $"{requestData}{timestamp}";

		var hashedBody = SHA256.HashData(Encoding.UTF8.GetBytes(fullText));
		var hash = Convert.FromBase64String(signature);

		var rsa = RSA.Create();
		rsa.ImportFromPem(publicKey);

		var result = rsa.VerifyHash(hashedBody, hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

		if (!result)
			_logger.LogTrace("VerifyHash failed: {fullText}, {signature}", fullText, signature);

		return result;
	}
}