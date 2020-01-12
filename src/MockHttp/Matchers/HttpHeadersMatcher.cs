using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using MockHttp.Http;
using MockHttp.Responses;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request by the request headers.
	/// </summary>
	public class HttpHeadersMatcher : ValueMatcher<HttpHeaders>
	{
		private static readonly HttpHeaderEqualityComparer EqualityComparer = new HttpHeaderEqualityComparer();
		private readonly RegexPatternMatcher _patternMatcher;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpHeadersMatcher"/> class using specified header <paramref name="name"/> and <paramref name="value"/>.
		/// </summary>
		/// <param name="name">The header name to match.</param>
		/// <param name="value">The header value to match.</param>
		/// <param name="allowWildcards"><see langword="true"/> to allow wildcards, or <see langword="false"/> if exact matching.</param>
		public HttpHeadersMatcher(string name, string value, bool allowWildcards = false)
			: base(new HttpHeadersCollection())
		{
			if (name is null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			_patternMatcher = allowWildcards ? new RegexPatternMatcher(value) : null;

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
			if (name is null)
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
			if (headers is null)
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
		public override bool IsMatch(MockHttpRequestContext requestContext)
		{
			return Value.All(h => IsMatch(h, requestContext.Request.Headers) || IsMatch(h, requestContext.Request.Content?.Headers));
		}

		/// <inheritdoc />
		public override bool IsExclusive => false;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Headers: {Value.ToString().TrimEnd('\r', '\n')}";
		}

		private bool IsMatch(KeyValuePair<string, IEnumerable<string>> expectedHeader, HttpHeaders headers)
		{
			return headers is { }
			 && headers.TryGetValues(expectedHeader.Key, out IEnumerable<string> vls)
			  &&
				(_patternMatcher != null && vls.Any(_patternMatcher.IsMatch)
			  ||
				_patternMatcher == null && EqualityComparer.Equals(
					expectedHeader,
					new KeyValuePair<string, IEnumerable<string>>(expectedHeader.Key, vls))
				);
		}
	}
}
