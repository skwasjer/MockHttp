using System;
using System.Collections.Generic;
using System.Net.Http;

namespace HttpClientMock.Matchers
{
	public class AnyMatcher : IHttpRequestMatcher
	{
		private readonly IReadOnlyCollection<IHttpRequestMatcher> _matchers;

		public AnyMatcher(IReadOnlyCollection<IHttpRequestMatcher> matchers)
		{
			_matchers = matchers ?? throw new ArgumentNullException(nameof(matchers));
		}

		public bool IsMatch(HttpRequestMessage request)
		{
			return _matchers.Any(request);
		}
	}
}