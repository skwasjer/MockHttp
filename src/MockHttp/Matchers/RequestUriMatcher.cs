using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MockHttp.Responses;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request by the request URI.
	/// </summary>
	public class RequestUriMatcher : HttpRequestMatcher
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Uri _requestUri;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string _formattedUri;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Regex _uriPatternMatcher;

		/// <summary>
		/// Initializes a new instance of the <see cref="RequestUriMatcher"/> class using specified <paramref name="uri"/>.
		/// </summary>
		/// <param name="uri">The request URI.</param>
		public RequestUriMatcher(Uri uri)
		{
			SetRequestUri(uri);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RequestUriMatcher"/> class using specified <paramref name="uriString"/>.
		/// </summary>
		/// <param name="uriString">The request URI or a URI wildcard.</param>
		/// <param name="allowWildcards"><see langword="true"/> to allow wildcards, or <see langword="false"/> if exact matching.</param>
		public RequestUriMatcher(string uriString, bool allowWildcards = true)
		{
			_formattedUri = uriString ?? throw new ArgumentNullException(nameof(uriString));

			if (allowWildcards && uriString.Length > 0 && uriString.Contains("*"))
			{
				_uriPatternMatcher = new Regex(GetMatchPattern(uriString));
			}
			else
			{
				// If no wildcards, then must be actual uri.
				SetRequestUri(new Uri(uriString, UriKind.RelativeOrAbsolute));
			}
		}

		private void SetRequestUri(Uri uri)
		{
			_requestUri = uri ?? throw new ArgumentNullException(nameof(uri));

			if (!_requestUri.IsAbsoluteUri && _requestUri.ToString()[0] != '/')
			{
				_requestUri = new Uri("/" + _requestUri, UriKind.Relative);
			}

			_formattedUri = _requestUri.ToString();
		}

		/// <inheritdoc />
		public override bool IsMatch(MockHttpRequestContext requestContext)
		{
			if (requestContext is null)
			{
				throw new ArgumentNullException(nameof(requestContext));
			}

			Uri requestUri = requestContext.Request.RequestUri;
			if (requestUri is null)
			{
				return false;
			}

			if (_uriPatternMatcher is null)
			{
				return IsAbsoluteUriMatch(requestUri) || IsRelativeUriMatch(requestUri);
			}

			return _uriPatternMatcher.IsMatch(requestUri.ToString());

		}

		private bool IsAbsoluteUriMatch(Uri uri)
		{
			return _requestUri.IsAbsoluteUri && uri.Equals(_requestUri);
		}

		private bool IsRelativeUriMatch(Uri uri)
		{
			return !_requestUri.IsAbsoluteUri
			 && uri.IsBaseOf(_requestUri)
			 && uri.ToString().EndsWith(_requestUri.ToString(), StringComparison.Ordinal);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"RequestUri: '{_formattedUri}'";
		}

		private static string GetMatchPattern(string value)
		{
			var pattern = new StringBuilder();
			bool startsWithWildcard = value[0] == '*';
			if (startsWithWildcard)
			{
				value = value.TrimStart('*');
				pattern.Append(".*");
			}
			else
			{
				pattern.Append("^");
			}

			bool endsWithWildcard = value.Length > 0 && value[value.Length - 1] == '*';
			if (endsWithWildcard)
			{
				value = value.TrimEnd('*');
			}

			IEnumerable<string> matchGroups = value
				.Split('*')
				.Where(s => !string.IsNullOrEmpty(s))
				.Select(s => $"({s})");

			pattern.Append(string.Join(".+", matchGroups));

			pattern.Append(endsWithWildcard ? ".*" : "$");

			return pattern.ToString();
		}
	}
}
