using System;
using System.Collections.Generic;
using System.Net.Http;

namespace MockHttp
{
	internal class InvokedHttpRequest : IInvokedHttpRequest
	{
		public InvokedHttpRequest(HttpCall setup, HttpRequestMessage request)
		{
			Setup = setup ?? throw new ArgumentNullException(nameof(setup));
			Request = request ?? throw new ArgumentNullException(nameof(request));
		}

		public HttpCall Setup { get; }

		public HttpRequestMessage Request { get; }

		public IReadOnlyCollection<IHttpRequestMatcher> Matchers => Setup.Matchers;
	}
}