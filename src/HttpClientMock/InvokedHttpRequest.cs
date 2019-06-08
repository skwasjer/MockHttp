using System.Collections.Generic;
using System.Net.Http;

namespace HttpClientMock
{
	internal class InvokedHttpRequest : IInvokedHttpRequest
	{
		public HttpCall Setup { get; internal set; }
		public HttpRequestMessage Request { get; internal set; }
		public IReadOnlyCollection<IHttpRequestMatcher> Matchers => Setup.Matchers;
	}
}