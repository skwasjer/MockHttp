using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using HttpClientMock.Utilities;
using Microsoft.Extensions.Primitives;

namespace HttpClientMock.HttpRequestMatchers
{
	public class HttpHeadersMatcher : IHttpRequestMatcher
	{
		public HttpHeadersMatcher(IEnumerable<KeyValuePair<string, StringValues>> headers)
		{
			ExpectedHeaders = headers?.ToDictionary(h => h.Key, h => h.Value) ?? throw new ArgumentNullException(nameof(headers));
		}

		public HttpHeadersMatcher(IEnumerable<KeyValuePair<string, string[]>> headers)
			: this(headers?.ToDictionary(h => h.Key, h => new StringValues(h.Value)))
		{
		}

		public HttpHeadersMatcher(IEnumerable<KeyValuePair<string, string>> headers)
			: this(headers?.ToDictionary(h => h.Key, h => new StringValues(h.Value)))
		{
		}

		public IDictionary<string, StringValues> ExpectedHeaders { get; }

		/// <inheritdoc />
		public bool IsMatch(HttpRequestMessage request)
		{
			if (request.Headers == null)
			{
				return false;
			}

			return ExpectedHeaders.All(h => IsMatch(h, request.Headers) || IsMatch(h, request.Content?.Headers));
		}

		private static bool IsMatch(KeyValuePair<string, StringValues> expectedHeader, HttpHeaders headers)
		{
			return headers != null 
				&& headers.TryGetValues(expectedHeader.Key, out IEnumerable<string> values)
				&& values.Any(v => 
					expectedHeader.Value.All(
						eh => HttpUtilities.ParseHttpHeaderValue(v).Contains(eh)
					)
				);
		}
	}
}
