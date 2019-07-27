using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using MockHttp.Http;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request by the request headers.
	/// </summary>
	public class HttpHeadersMatcher : ValueMatcher<HttpHeaders>
	{
		private static readonly HttpHeaderEqualityComparer EqualityComparer = new HttpHeaderEqualityComparer();

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpHeadersMatcher"/> class using specified header <paramref name="name"/> and <paramref name="value"/>.
		/// </summary>
		/// <param name="name">The header name to match.</param>
		/// <param name="value">The header value to match.</param>
		public HttpHeadersMatcher(string name, string value)
			: base(new HttpHeadersCollection())
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			Value.Add(name, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpHeadersMatcher"/> class using specified header <paramref name="name"/> and <paramref name="values"/>.
		/// </summary>
		/// <param name="name">The header name to match.</param>
		/// <param name="values">The header values to match.</param>
		public HttpHeadersMatcher(string name, IEnumerable<string> values)
			: base(new HttpHeadersCollection())
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			Value.Add(name, values);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpHeadersMatcher"/> class using specified <paramref name="headers"/>.
		/// </summary>
		/// <param name="headers">The headers to match.</param>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpHeadersMatcher"/> class using specified <paramref name="headers"/>.
		/// </summary>
		/// <param name="headers">The headers to match.</param>
		public HttpHeadersMatcher(IEnumerable<KeyValuePair<string, string>> headers)
			: this(headers?.ToDictionary(h => h.Key, h => Enumerable.Repeat(h.Value, 1)))
		{
		}

		/// <inheritdoc />
		public override bool IsMatch(HttpRequestMessage request)
		{
			return Value.All(h => IsMatch(h, request.Headers) || IsMatch(h, request.Content?.Headers));
		}

		/// <inheritdoc />
		public override bool IsExclusive => false;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Headers: {Value.ToString().TrimEnd('\r', '\n')}";
		}

		private static bool IsMatch(KeyValuePair<string, IEnumerable<string>> expectedHeader, HttpHeaders headers)
		{
			return headers != null
				&& headers.TryGetValues(expectedHeader.Key, out IEnumerable<string> vls) 
				&& EqualityComparer.Equals(
					expectedHeader, 
					new KeyValuePair<string, IEnumerable<string>>(expectedHeader.Key, vls)
				);
		}
	}
}
