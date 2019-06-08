using System.Collections.Generic;
using System.Net.Http;

namespace HttpClientMock
{
	public interface IInvokedHttpRequest
	{
		HttpRequestMessage Request { get; }
		IReadOnlyCollection<IHttpRequestMatcher> Matchers { get; }
	}
}