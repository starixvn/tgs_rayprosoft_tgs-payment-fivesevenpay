using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.Payment.FiveSevenPay.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
	.AddPaymentClient()
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig => natsConfig
		.ConfigQueueOptions((opt, _) => opt.Url = "nats://localhost:4222"))
	.AddNatsGlobPatternExchange("*");

var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var merchantKey = "2b223bce3f024a3f885d213770a0fc5e";

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

var publicKey = @"
-----BEGIN RSA PUBLIC KEY-----
MIGJAoGBANgrjHugVHMpRJEex9MDqTuQGAIFcKupQsBW2ogQy6KSG4T7BOhW6jOd
0NMBvsMQg5bMPLaBenOBPS7WpKUGVkZK0f/utD5/FDa6Gei5LokBn1n3Hd1zuwoI
D4KpzlnaurtlY0L8Yj4RDYu9sYxscRY5XuYpSKo1dISFQQ2fnQ73AgMBAAE=
-----END RSA PUBLIC KEY-----
";

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var paymentClient = app.Services.GetRequiredService<IPaymentClient>();

//var depositSettings = new FiveSevenPayDepositSettings(
//	merchantKey,
//	Convert.ToBase64String(Encoding.UTF8.GetBytes(privateKey)),
//	Convert.ToBase64String(Encoding.UTF8.GetBytes(publicKey)),
//	"PHP",
//	FiveSevenPaymentType.GCash,
//	Guid.NewGuid());

//var applyInfo = JsonSerializer.Serialize(depositSettings, FiveSevenPayUtility.SerializerOptions);
//logger.LogInformation("ApplyInfo: {applyInfo}", applyInfo);

//var depositReturn = await paymentClient.DepositAsync(
//	"tgs.payment.fivesevenpay.deposit",
//	100M,
//	applyInfo).ConfigureAwait(false);

//logger.LogInformation("DepositReturn: {depositReturn}", depositReturn);

var withdrawSettings = new FiveSevenPayWithdrawSettings(
	merchantKey,
	Convert.ToBase64String(Encoding.UTF8.GetBytes(privateKey)),
	Convert.ToBase64String(Encoding.UTF8.GetBytes(publicKey)),
	"PHP",
	FiveSevenPaymentType.GCash,
	Guid.NewGuid(),
	"Gcash",
	"1111111",
	"1111111");

var applyInfo = JsonSerializer.Serialize(withdrawSettings, FiveSevenPayUtility.SerializerOptions);
logger.LogInformation("ApplyInfo: {applyInfo}", applyInfo);

var withdrawReturn = await paymentClient.WithdrawAsync(
	"tgs.payment.fivesevenpay.withdraw",
	20M,
	applyInfo).ConfigureAwait(false);

logger.LogInformation("WithdrawReturn: {withdrawReturn}", withdrawReturn);

Console.ReadLine();
