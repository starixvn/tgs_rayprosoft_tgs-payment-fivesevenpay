using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.Payment.FiveSevenPay.ApplicationServices;
using Tgs.Payment.FiveSevenPay.Client;
using Tgs.Payment.FiveSevenPay.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.FiveSevenPay.Controllers;

[Route("[controller]")]
[ApiController]
public class DepositController : ControllerBase
{
	private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);
	private readonly ILogger<DepositController> _logger;

	public DepositController(
		ILogger<DepositController> logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	[HttpPost("callback", Name = "DepositCallback")]
	[Produces(MediaTypeNames.Text.Plain)]
	public async ValueTask<string> DepositCallbackAsync(
		[FromServices] FiveSevenPayHasher hasher,
		[FromServices] IGetService<GetDepositFormQuery, DepositForm?> getService,
		[FromServices] IPaymentServiceClient paymentServiceClient,
		[FromServices] IPaymentClient paymentClient,
		[FromHeader(Name = "signature")] string signature,
		[FromHeader(Name = "timestamp")] long timestamp,
		JsonNode node)
	{
		var viewModel = node.Deserialize<DepositCallbackViewModel>(_jsonSerializerOptions)!;

		var depositForm = await getService.GetAsync(
				new(viewModel.FormId), 
				HttpContext.RequestAborted).ConfigureAwait(false);

		if (depositForm == null)
		{
			_logger.LogWarning("Can't found deposit form({id}).", viewModel.FormId);
			return "success";
		}

		var settings = depositForm.ApplyInfo!.Settings;

		var publicKey = Encoding.UTF8.GetString(Convert.FromBase64String(settings.PublicKeyBase64));

		var requestData = node.ToJsonString();
		var isValid = hasher.VerifyHash(publicKey, requestData, signature, timestamp);

		if (!isValid)
		{
			_logger.LogWarning("Sign value invalid of deposit form({id}). data: {data}, signature: {signature}, timestamp: {timestamp}", viewModel.FormId, requestData, signature, timestamp);
			return "fail";
		}

		var paymentReturn = new FiveSevenPaymentReturn(
			viewModel.FormId,
			viewModel.Currency,
			viewModel.Amount,
			viewModel.OriginalAmount,
			viewModel.AgreeAmount,
			viewModel.Status,
			viewModel.OrderType,
			viewModel.Remark);

		var fsm = PaymentStateMachineFactory.CreateDepositStateMachine(depositForm.Status);

		switch (paymentReturn.Status)
		{
			case "Payed":
				if (fsm.CanFire(PaymentTrigger.PaymentDepositCompleted))
				{
					await fsm.FireAsync(PaymentTrigger.PaymentDepositCompleted).ConfigureAwait(false);

					await paymentServiceClient.SubmitDepositFormStatusAsync(
						depositForm.Id,
						paymentReturn: JsonSerializer.Serialize(
							paymentReturn,
							FiveSevenPayUtility.SerializerOptions),
						cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

					await paymentClient.ApproveDepositAsync(
						depositForm.Id,
						depositForm.BrandId,
						paymentReturn.Amount,
						HttpContext.RequestAborted).ConfigureAwait(false);
				}

				break;

			case "OverTime" or "Canceled" or "Failed":
				if (fsm.CanFire(PaymentTrigger.Reject))
				{
					await fsm.FireAsync(PaymentTrigger.Reject).ConfigureAwait(false);

					await paymentServiceClient.SubmitDepositFormStatusAsync(
						depositForm.Id,
						paymentReturn: JsonSerializer.Serialize(
							paymentReturn,
							FiveSevenPayUtility.SerializerOptions),
						cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

					await paymentClient.RejectDepositAsync(
						depositForm.Id,
						depositForm.BrandId,
						HttpContext.RequestAborted).ConfigureAwait(false);
				}

				break;
		}

		return "success";
	}
}
