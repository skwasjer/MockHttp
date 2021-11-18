using System;
using System.Collections.Generic;
using System.Net.Http;
using MockHttp.Matchers;

namespace MockHttp
{
	internal sealed class InvokedHttpRequest : IInvokedHttpRequest
	{
		private bool _markedAsVerified;

		public InvokedHttpRequest(HttpCall setup, HttpRequestMessage request)
		{
			Setup = setup ?? throw new ArgumentNullException(nameof(setup));
			Request = request ?? throw new ArgumentNullException(nameof(request));
		}

		internal HttpCall Setup { get; }

		public HttpRequestMessage Request { get; }

		public IReadOnlyCollection<IAsyncHttpRequestMatcher> Matchers => Setup.Matchers;

		internal bool IsVerified => Setup.IsVerified || _markedAsVerified;

		internal void MarkAsVerified()
		{
			_markedAsVerified = true;
		}
	}
}
