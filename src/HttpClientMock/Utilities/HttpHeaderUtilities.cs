using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace HttpClientMock.Utilities
{
	internal static class HttpUtilities
	{
		public static IEnumerable<KeyValuePair<string, StringValues>> ParseHttpHeaders(string headers)
		{
			var headerList = new List<KeyValuePair<string, StringValues>>();

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
					if (hvp.Length != 2)
					{
						throw new InvalidOperationException($"Unexpected header format: {header}");
					}

					headerList.Add(new KeyValuePair<string, StringValues>(hvp[0], ParseHttpHeaderValue(hvp[1])));
				}
			}

			return headerList
				.GroupBy(hvp => hvp.Key)
				.Select(g => 
					new KeyValuePair<string, StringValues>(
						g.Key,
						new StringValues(g.SelectMany(gv => gv.Value).ToArray())
					)
				);
		}

		public static StringValues ParseHttpHeaderValue(string headerValue)
		{
			string[] values = headerValue
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(v => v.Trim())
				.ToArray();
			return new StringValues(values);
		}
	}
}
