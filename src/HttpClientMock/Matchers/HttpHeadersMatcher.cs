using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using HttpClientMock.Http;

namespace HttpClientMock.Matchers
{
	public class HttpHeadersMatcher : IHttpRequestMatcher
	{
		public HttpHeadersMatcher(string name, string value)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			ExpectedHeaders = new HttpHeadersCollection
			{
				{ name, value }
			};
		}
		public HttpHeadersMatcher(string name, IEnumerable<string> values)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			ExpectedHeaders = new HttpHeadersCollection
			{
				{ name, values }
			};
		}

		public HttpHeadersMatcher(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
		{
			if (headers == null)
			{
				throw new ArgumentNullException(nameof(headers));
			}

			ExpectedHeaders = new HttpHeadersCollection();
			foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
			{
				ExpectedHeaders.Add(header.Key, header.Value);
			}
		}

		public HttpHeadersMatcher(IEnumerable<KeyValuePair<string, string>> headers)
			: this(headers?.ToDictionary(h => h.Key, h => Enumerable.Repeat(h.Value, 1)))
		{
		}

		public HttpHeaders ExpectedHeaders { get; }

		/// <inheritdoc />
		public bool IsMatch(HttpRequestMessage request)
		{
			return ExpectedHeaders.All(h => IsMatch(h, request.Headers) || IsMatch(h, request.Content?.Headers));
		}

		public override string ToString()
		{
			return $"Headers: {ExpectedHeaders.ToString().TrimEnd('\r', '\n')}";
		}

		private static bool IsMatch(KeyValuePair<string, IEnumerable<string>> expectedHeader, HttpHeaders headers)
		{
			return headers != null
				&& headers.TryGetValues(expectedHeader.Key, out IEnumerable<string> values)
				&& values.Any(v =>
					expectedHeader.Value.All(
						eh => HttpHeadersCollection.ParseHttpHeaderValue(v).Contains(eh)
					)
				);
		}
	}
}
