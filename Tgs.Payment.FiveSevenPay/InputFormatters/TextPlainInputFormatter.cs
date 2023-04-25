using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Tgs.Payment.FiveSevenPay.InputFormatters;

public class TextPlainInputFormatter : InputFormatter
{
	public TextPlainInputFormatter()
	{
		SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));
	}

	public override async Task<InputFormatterResult> ReadRequestBodyAsync(
		InputFormatterContext context)
	{
		if (context == null)
		{
			throw new ArgumentNullException(nameof(context));
		}
		
		using var reader = new StreamReader(context.HttpContext.Request.Body, Encoding.UTF8);
		var content = await reader.ReadToEndAsync();
		return await InputFormatterResult.SuccessAsync(content);
	}

	protected override bool CanReadType(Type type)
	{
		return type == typeof(string);
	}
}
