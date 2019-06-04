using System;
using System.Net.Http;

namespace HttpClientMock.HttpRequestMatchers
{
	public class UrlMatcher : IHttpRequestMatcher
	{
		public bool IsMatch(HttpRequestMessage request)
		{
			throw new NotImplementedException();
		}
	}
}
