using System.Text;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.FiveSevenPay.ViewModels;

namespace Tgs.Payment.FiveSevenPay.Controllers;

[Route("[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "DataConverter")]
public class DataConverterController : ControllerBase
{
	[HttpPost("base64:from")]
	public ValueTask<string> ConvertFromBase64StringAsync(
		FromBase64ViewModel viewModel)
	{
		var data = Convert.FromBase64String(viewModel.Base64String);
		return ValueTask.FromResult(Encoding.UTF8.GetString(data));
	}

	[HttpPost("base64:to")]
	public async ValueTask<string> ConvertToBase64StringAsync(
		[FromForm(Name = "file")] IFormFile file)
	{
		await using var stream = file.OpenReadStream();
		var streamReader = new StreamReader(stream);
		var value = await streamReader.ReadToEndAsync();
		
		var data = Encoding.UTF8.GetBytes(value);
		return Convert.ToBase64String(data);
	}
}
