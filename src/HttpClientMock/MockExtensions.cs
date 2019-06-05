using HttpClientMock.HttpRequestMatchers;

namespace HttpClientMock
{
	public static class MockExtensions
	{
		public static IMockedHttpRequest WhenRequesting(this IMockHttpRequestBuilder builder, string requestUri)
		{
			return builder.WhenRequesting(new MockedHttpRequest(builder).With(new UrlMatcher(requestUri)));
		}
	}
}