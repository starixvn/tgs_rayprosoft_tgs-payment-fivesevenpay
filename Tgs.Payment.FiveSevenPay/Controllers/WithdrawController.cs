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
public class WithdrawController : ControllerBase
{
	private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);
	private readonly ILogger<WithdrawController> _logger;

	public WithdrawController(
		ILogger<WithdrawController> logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	[HttpPost("callback", Name = "WithdrawCallback")]
	[Produces(MediaTypeNames.Text.Plain)]
	public async ValueTask<string> WithdrawCallbackAsync(
		[FromServices] FiveSevenPayHasher hasher,
		[FromServices] IGetService<GetWithdrawFormQuery, WithdrawForm?> getService,
		[FromServices] IPaymentServiceClient paymentServiceClient,
		[FromServices] IPaymentClient paymentClient,
		[FromHeader(Name = "signature")] string signature,
		[FromHeader(Name = "timestamp")] long timestamp,
		JsonNode node)
	{
		var viewModel = node.Deserialize<WithdrawCallbackViewModel>(_jsonSerializerOptions)!;

		var withdrawForm = await getService.GetAsync(
				new(viewModel.FormId),
				HttpContext.RequestAborted).ConfigureAwait(false);

		if (withdrawForm == null)
		{
			_logger.LogWarning("Can't found withdraw form({id}).", viewModel.FormId);
			return "success";
		}

		var settings = withdrawForm.ApplyInfo!.Settings;

		var publicKey = Encoding.UTF8.GetString(Convert.FromBase64String(settings.PublicKey));

		var requestData = node.ToJsonString();
		var isValid = hasher.VerifyHash(publicKey, requestData, signature, timestamp);

		if (!isValid)
		{
			_logger.LogWarning("Sign value invalid of withdraw form({id}). data: {data}, signature: {signature}, timestamp: {timestamp}", viewModel.FormId, requestData, signature, timestamp);
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

		var fsm = PaymentStateMachineFactory.CreateWithdrawStateMachine(withdrawForm.Status);

		switch (paymentReturn.Status)
		{
			case "Payed":
				if (fsm.CanFire(PaymentTrigger.PaymentWithdrawCompleted))
				{
					await fsm.FireAsync(PaymentTrigger.PaymentWithdrawCompleted).ConfigureAwait(false);

					await paymentServiceClient.SubmitWithdrawFormStatusAsync(
						withdrawForm.Id,
						paymentReturn: JsonSerializer.Serialize(
							paymentReturn,
							FiveSevenPayUtility.SerializerOptions),
						cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

					await paymentClient.PaidWithdrawAsync(
						withdrawForm.Id,
						withdrawForm.BrandId,
						HttpContext.RequestAborted).ConfigureAwait(false);
				}

				break;

			case "OverTime" or "Canceled" or "Failed":
				if (fsm.CanFire(PaymentTrigger.Reject))
				{
					await fsm.FireAsync(PaymentTrigger.Reject).ConfigureAwait(false);

					await paymentServiceClient.SubmitWithdrawFormStatusAsync(
						withdrawForm.Id,
						paymentReturn: JsonSerializer.Serialize(
							paymentReturn,
							FiveSevenPayUtility.SerializerOptions),
						cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

					await paymentClient.RejectWithdrawAsync(
						withdrawForm.Id,
						withdrawForm.BrandId,
						HttpContext.RequestAborted).ConfigureAwait(false);
				}

				break;
		}

		return "success";
	}
}
