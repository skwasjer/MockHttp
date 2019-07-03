using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using HttpClientMock.Http;

namespace HttpClientMock.Matchers
{
	public class HttpHeadersMatcher : ValueMatcher<HttpHeaders>
	{
		public HttpHeadersMatcher(string name, string value)
			: base(new HttpHeadersCollection())
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			Value.Add(name, value);
		}

		public HttpHeadersMatcher(string name, IEnumerable<string> values)
			: base(new HttpHeadersCollection())
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			Value.Add(name, values);
		}

		public HttpHeadersMatcher(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
			: base(new HttpHeadersCollection())
		{
			if (headers == null)
			{
				throw new ArgumentNullException(nameof(headers));
			}

			foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
			{
				Value.Add(header.Key, header.Value);
			}
		}

		public HttpHeadersMatcher(IEnumerable<KeyValuePair<string, string>> headers)
			: this(headers?.ToDictionary(h => h.Key, h => Enumerable.Repeat(h.Value, 1)))
		{
		}

		/// <inheritdoc />
		public override bool IsMatch(HttpRequestMessage request)
		{
			return Value.All(h => IsMatch(h, request.Headers) || IsMatch(h, request.Content?.Headers));
		}

		public override string ToString()
		{
			return $"Headers: {Value.ToString().TrimEnd('\r', '\n')}";
		}

		private static bool IsMatch(KeyValuePair<string, IEnumerable<string>> expectedHeader, HttpHeaders headers)
		{
			return headers != null
				&& headers.TryGetValues(expectedHeader.Key, out IEnumerable<string> values)
				&& values.Any(v => 
					expectedHeader.Value
						.SelectMany(HttpHeadersCollection.ParseHttpHeaderValue)
						.All(
							eh => HttpHeadersCollection.ParseHttpHeaderValue(v).Contains(eh)
						)
				);
		}
	}
}
