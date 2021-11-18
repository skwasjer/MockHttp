using System.Net.Http.Headers;

namespace MockHttp.Http;

internal sealed class HttpHeadersCollection : HttpHeaders
{
	public static HttpHeaders Parse(string headers)
	{
		if (headers is null)
		{
			throw new ArgumentNullException(nameof(headers));
		}

		var httpHeaders = new HttpHeadersCollection();
		if (headers.Length == 0)
		{
			return httpHeaders;
		}

		using var sr = new StringReader(headers);
		while (true)
		{
			string header = sr.ReadLine();
			if (header is null)
			{
				break;
			}

			if (string.IsNullOrWhiteSpace(header))
			{
				continue;
			}

			string[] hvp = header.Split(new[] { ':' }, 2, StringSplitOptions.None);

			string fieldName = hvp.Length > 0 ? hvp[0] : null;
			string fieldValue = hvp.Length > 1 ? hvp[1] : null;
			httpHeaders.Add(fieldName, ParseHttpHeaderValue(fieldValue));
		}

		return httpHeaders;
	}

	internal static IEnumerable<string> ParseHttpHeaderValue(string headerValue)
	{
		if (headerValue is null)
		{
			throw new ArgumentNullException(nameof(headerValue), "The value cannot be null or empty.");
		}

		return headerValue
			.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
			.Select(v => v.Trim())
			.Where(v => v.Length > 0)
			.ToArray();
	}
}