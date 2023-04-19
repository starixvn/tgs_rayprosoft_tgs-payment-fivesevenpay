using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
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

		var data = Encoding.UTF8.GetBytes(privateKey);
		var pemBase64 = Convert.ToBase64String(data);

		var merchantKey = "2b223bce3f024a3f885d213770a0fc5e";

		var dict = new Dictionary<string, object?>
		{
			["merchantKey"] = merchantKey,
			["amount"] = 100,
			["currency"] = "PHP",
			["outID"] = "b5c88f52-85f0-428c-a914-3124b0e4feb4",
			["verificationType"] = "Amount",
			["payType"] = "Gcash",
			["notifyUrl"] = "http://localhost/payment",
		};

		var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

		using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
		var logger = loggerFactory.CreateLogger<FiveSevenPayHasher>();

		//var sut = new FiveSevenPayHasher(NullLogger<FiveSevenPayHasher>.Instance);
		var sut = new FiveSevenPayHasher(logger);

		var actual = sut.ComputeHash(pemBase64, dict, timestamp);

		_testOutputHelper.WriteLine("signature: {0}", actual);

//		var publicKey = @"
//-----BEGIN RSA PUBLIC KEY-----
//MIGJAoGBANgrjHugVHMpRJEex9MDqTuQGAIFcKupQsBW2ogQy6KSG4T7BOhW6jOd
//0NMBvsMQg5bMPLaBenOBPS7WpKUGVkZK0f/utD5/FDa6Gei5LokBn1n3Hd1zuwoI
//D4KpzlnaurtlY0L8Yj4RDYu9sYxscRY5XuYpSKo1dISFQQ2fnQ73AgMBAAE=
//-----END RSA PUBLIC KEY-----
//";

		//var signature = Convert.FromBase64String(actual);

		//var json = JsonSerializer.Serialize(
		//	dict.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
		//		.ToDictionary(p => p.Key,
		//			p => WebUtility.UrlEncode(p.Value)));

		//var fullText = $"{json}{timestamp}";

		//_testOutputHelper.WriteLine("full text: {0}", fullText);

		//var hashedBody = SHA256.HashData(Encoding.UTF8.GetBytes(fullText));

		//var rsa = RSA.Create();
		
		//var result = rsa.VerifyHash(hashedBody, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

		//Assert.True(result);
	}

	//	[Fact]
	//	public void HashTest1()
	//	{
	//		var s = "HelloWorld";

	//		var privateKey = @"-----BEGIN RSA PRIVATE KEY-----
	//MIICWwIBAAKBgQC7qh3f+UFIEdbP8voKBiwiXx3MRkNnp8xcWqQ0CowXBaCaWtAn
	///Xasf5dKMWUt6ihcK7Sm2h63wTUCrAZiJiZRHV52sVvsbvM6+3eBAh+jZu/P4lhk
	//+ZJOekz4QNQicNsuweH8cyThHmiBchA3bszCD3s8BI5OJ41MXWqp47H3MwIDAQAB
	//AoGANWvX1Uo/KyTo4YlpZDEdnys0cFIQHhHGOwQSej4GYwdXEJmJZ9jfT3uCtbwZ
	//8VgW5SYL5vNwix6U2I3fw1DY1wGVWPxUAtuGHW/BjNoncVqau24nVMKDpUsiNoAT
	//LLRJzcXRstEdJ/mbKSQSsYvTMhQPRQCPIx7KgPJ3eEy0JwECQQDXFyOL4OKur0eM
	//vwihG9w2M1biuOIRZAfd6jN2em7pQDZg5xJRLHnYH2qf4IaTkxsh3xxtCk6fEMB0
	//CjKapVndAkEA31uZKUEWRaGT2I89mPEteXZaI5eRQy4rLvHlQMbdJH9HNDLISGSE
	//0lFk/id9gEPHwv91AC+jkYVVEI+97KpsTwJACW3ZQBOOvGuwrkndnfpzr+iWQYAr
	//YOnKsn7G3MXHQPBwsdTPZ8GUYdYWvYWBQMztHpICVAFkqvJ4DV45E7B1/QJACNc1
	//Tr62xPbKRU4WIBGbvS0EEFiVdVVEfE0yx8eXRTSMevJvnv3ri++Q0cZ6XzNPmtZF
	//qUY5cS8X2Nn3qIBuXQJAANoUTkL8nejtdXoNFTvWNheRP8J10d1g3wNkvi0Qe+OK
	//jNcFxveAuXEHUBab7hrUfQtlthiYME9l/AP3PkyvkA==
	//-----END RSA PRIVATE KEY-----";

	//		var rsa = RSA.Create();
	//		rsa.ImportFromPem(privateKey);

	//		var hashed = SHA256.HashData(Encoding.UTF8.GetBytes(s));
	//		var signature = rsa.SignHash(hashed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	//		//var signature = rsa.SignData(hashed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

	//		//signature.Dump();

	//		var publicKey = @"-----BEGIN PUBLIC KEY-----
	//MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDYK4x7oFRzKUSRHsfTA6k7kBgC
	//BXCrqULAVtqIEMuikhuE+wToVuozndDTAb7DEIOWzDy2gXpzgT0u1qSlBlZGStH/
	//7rQ+fxQ2uhnouS6JAZ9Z9x3dc7sKCA+Cqc5Z2rq7ZWNC/GI+EQ2LvbGMbHEWOV7m
	//KUiqNXSEhUENn50O9wIDAQAB
	//-----END PUBLIC KEY-----";

	//		var rsa1 = RSA.Create();
	//		rsa1.ImportFromPem(publicKey);
	//		//rsa1.ImportRSAPublicKey(Encoding.UTF8.GetBytes(publicKey), out _);

	//		var result = rsa1.VerifyHash(hashed, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	//		//var result = rsa1.VerifyData(hashed, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

	//		Assert.True(result);
	//	}

	//	[Fact]
	//	public void HashTest2()
	//	{
	//		var s = "HelloWorld";

	//		var privateKey = @"-----BEGIN RSA PRIVATE KEY-----
	//        MIICXAIBAAKBgQC7pbDRvuqRL0EsKjYGqPZWu4VFKI1iilisb3+yW5guIP4I+T8q
	//        hP8dcUugiwa2fDwGgl3Y+PHASGdCfvYOfMefWlNUoGeLv4Tx6nqvq2OcVzRhYjUf
	//        IpRE7z81xhmKtvxxLTNzTAoHN6rWqarKYMFydR/sD+oJs7LnnJoWieh1QQIDAQAB
	//        AoGAPeQ4nfXKiHh9loOVrjysg472NglaGNZoyPc9tyZe21gmce9D1lJnkt57g0hX
	//        vnjbk4oMSjRSCInZBSW7IqwlauiYwJra5O7MbLFaCTWDDnwFB+Zcz9E2PbPdFQFl
	//        jyYfh0gm9zvo79oyqESXtfYaMfx45zPbJGOkhiPcjZDsZcECQQDyy2qz6L6JALIY
	//        L1CtY6NTSm33rXv6Y+RzTBZovkK9OvMODvklsOhbcNQx0o7vQ5600dbHORwteCcm
	//        cHMgFDGnAkEAxdpsD2omWPzRYCRmk72aqRtccp9AE9eE75mUG6sMt3NDqwGeTuWV
	//        EH70XC3yIwqFmkc3zzNjZqsxTEDtuRtu1wJBAJoAHpko2poJv+0JLfIczf7JqgC8
	//        oHPMop3jOB+N9sUSPBLBupSGpotBgMZtWM44pNTqeIH7Hn1UxfhiwRMq2+cCQDwZ
	//        K8fG450WNncwt2PbLRZ+9CbxDqK4TW4GRYHeBD/ZKE3ScQbgH9Zh6dHyNuHD+W8y
	//        gNZUcrYl/BSAiHU4ywMCQDok1QgvVmc8jma9qK2dxmd4IR8varRPti4VQAMumdaE
	//        4s1Ofdw+8Bl2FATkdQZDLUQbWat4E5l8uGLfYZAHSow=
	//        -----END RSA PRIVATE KEY-----";

	//		var rsa = RSA.Create();
	//		rsa.ImportFromPem(privateKey);

	//		var hashed = SHA256.HashData(Encoding.UTF8.GetBytes(s));
	//		var signature = rsa.SignHash(hashed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	//		//var signature = rsa.SignData(hashed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

	//		//signature.Dump();

	//		var publicKey = @"-----BEGIN PUBLIC KEY-----
	//        MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC7pbDRvuqRL0EsKjYGqPZWu4VF
	//        KI1iilisb3+yW5guIP4I+T8qhP8dcUugiwa2fDwGgl3Y+PHASGdCfvYOfMefWlNU
	//        oGeLv4Tx6nqvq2OcVzRhYjUfIpRE7z81xhmKtvxxLTNzTAoHN6rWqarKYMFydR/s
	//        D+oJs7LnnJoWieh1QQIDAQAB
	//        -----END PUBLIC KEY-----";

	//		var rsa1 = RSA.Create();
	//		rsa1.ImportFromPem(publicKey);
	//		//rsa1.ImportRSAPublicKey(Encoding.UTF8.GetBytes(publicKey), out _);

	//		var result = rsa1.VerifyHash(hashed, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	//		//var result = rsa1.VerifyData(hashed, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

	//		Assert.True(result);
	//	}

	[Fact]
	public async Task HashTest4()
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
			["outID"] = "b5c88f52-85f0-428c-a914-3124b0e4feb4",
			["verificationType"] = "Amount",
			["payType"] = "Gcash",
			["remark"] = ""
			//["notifyUrl"] = WebUtility.UrlEncode("http://127.0.0.1/payment"),
		};

		dict = dict.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
			.ToDictionary(p => p.Key,
				p => WebUtility.UrlEncode(p.Value));

		var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

		//var json = JsonSerializer.Serialize(dict);

		var json = JsonSerializer.Serialize(dict, jsonSerializerOptions);

		var fullText = $"{json}{timestamp}";

		_testOutputHelper.WriteLine("FullText: {0}", fullText);

		var shaHashed = SHA256.HashData(Encoding.UTF8.GetBytes(fullText));

		var rsa = RSA.Create();
		rsa.ImportFromPem(privateKey);
		//rsa.ImportRSAPrivateKey(Encoding.UTF8.GetBytes(privateKey), out _);

		var hash = rsa.SignHash(shaHashed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		//var hash = rsa.SignData(shaHashed, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

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
