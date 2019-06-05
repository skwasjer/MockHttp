namespace HttpClientMock
{
	public interface IMockHttpRequestBuilder
	{
		IMockedHttpRequest WhenRequesting(IMockedHttpRequest mockedRequest);
	}
}