using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

namespace HttpClientMock.Http
{
	internal class HttpHeadersCollection : HttpHeaders
	{
		internal HttpHeadersCollection()
		{
			
		}

		public static HttpHeaders Parse(string headers)
		{
			if (headers == null)
			{
				throw new ArgumentNullException(nameof(headers));
			}

			var httpHeaders = new HttpHeadersCollection();
			if (headers.Length == 0)
			{
				return httpHeaders;
			}

			using (var sr = new StringReader(headers))
			{
				while (true)
				{
					string header = sr.ReadLine();
					if (string.IsNullOrWhiteSpace(header))
					{
						break;
					}

					string[] hvp = header.Split(new[] { ':' }, 2, StringSplitOptions.None);

					string fieldName = hvp.Length > 0 ? hvp[0] : null;
					string fieldValue = hvp.Length > 1 ? hvp[1] : null;
					httpHeaders.Add(fieldName, ParseHttpHeaderValue(fieldValue));
				}
			}

			return httpHeaders;
		}

		internal static IEnumerable<string> ParseHttpHeaderValue(string headerValue)
		{
			if (headerValue == null)
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
}