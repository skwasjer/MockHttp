using System;
using System.Collections.Generic;
using System.Linq;
using MockHttp.Matchers;

namespace MockHttp.Http
{
	internal sealed class HttpHeaderEqualityComparer : IEqualityComparer<KeyValuePair<string, IEnumerable<string>>>
	{
		private readonly PatternMatcher _valuePatternMatcher;
		private readonly bool _isOnlyMatchingHeaderName;

		public HttpHeaderEqualityComparer()
		{
		}

		public HttpHeaderEqualityComparer(PatternMatcher valuePatternMatcher)
		{
			_valuePatternMatcher = valuePatternMatcher ?? throw new ArgumentNullException(nameof(valuePatternMatcher));
		}

		public HttpHeaderEqualityComparer(bool isOnlyMatchingHeaderName)
		{
			_isOnlyMatchingHeaderName = isOnlyMatchingHeaderName;
		}

		public bool Equals(KeyValuePair<string, IEnumerable<string>> x, KeyValuePair<string, IEnumerable<string>> y)
		{
			if (x.Key != y.Key)
			{
				return false;
			}

			if (_isOnlyMatchingHeaderName)
			{
				return true;
			}

			if (y.Value.Any(
				yValue => x.Value
					.SelectMany(HttpHeadersCollection.ParseHttpHeaderValue)
					.All(xValue =>
					{
						string[] headerValues = HttpHeadersCollection.ParseHttpHeaderValue(yValue).ToArray();
						return headerValues.Contains(xValue) || _valuePatternMatcher != null && headerValues.Any(_valuePatternMatcher.IsMatch);
					})
				))
			{
				return true;
			}

			return !x.Value.Any();
		}

		public int GetHashCode(KeyValuePair<string, IEnumerable<string>> obj)
		{
			throw new NotImplementedException();
		}
	}
}
