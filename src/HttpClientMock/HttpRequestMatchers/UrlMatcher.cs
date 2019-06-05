using System;
using System.Net.Http;

namespace HttpClientMock.HttpRequestMatchers
{
	public class UrlMatcher : IHttpRequestMatcher
	{
		private readonly string _requestUri;

		public UrlMatcher(string requestUri)
		{
			_requestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
		}

		public bool IsMatch(HttpRequestMessage request)
		{
			return request.RequestUri.ToString() == _requestUri;
		}

		public override string ToString()
		{
			return $"Uri: {_requestUri}";
		}
	}
}
