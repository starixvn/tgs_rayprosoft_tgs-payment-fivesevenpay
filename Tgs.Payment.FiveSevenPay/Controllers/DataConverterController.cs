using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Tgs.Payment.FiveSevenPay.Controllers;

[Route("[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "DataConverter")]
public class DataConverterController : ControllerBase
{
	[HttpPost("base64:to")]
	[Consumes("text/plain")]
	public ValueTask<string> ConvertToBase64StringAsync(
		[FromBody] string original)
	{
		var value = original.ReplaceLineEndings();

		var data = Encoding.UTF8.GetBytes(value);
		return ValueTask.FromResult(Convert.ToBase64String(data));
	}

	[HttpPost("base64:from")]
	[Consumes("text/plain")]
	public ValueTask<string> ConvertFromBase64StringAsync(
		[FromBody] string base64)
	{
		var data = Convert.FromBase64String(base64);
		return ValueTask.FromResult(Encoding.UTF8.GetString(data));
	}
}
