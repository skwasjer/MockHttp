using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientMock
{
	internal sealed class MockedHttpRequest : IMockedHttpRequest, IFluentInterface
	{
		private readonly List<IHttpRequestMatcher> _matchers;
		private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _response;
		private readonly Action<HttpRequestMessage, HttpResponseMessage> _callback;

		public MockedHttpRequest(IEnumerable<IHttpRequestMatcher> matchers, Func<HttpRequestMessage, Task<HttpResponseMessage>> response, Action<HttpRequestMessage, HttpResponseMessage> callback)
		{
			_matchers = matchers?.ToList() ?? throw new ArgumentNullException(nameof(matchers));
			_response = response ?? throw new ArgumentNullException(nameof(response));
			_callback = callback;
		}

		public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			HttpResponseMessage response = await _response(request).ConfigureAwait(false);

			_callback?.Invoke(request, response);

			return response;
		}

		public bool Matches(HttpRequestMessage request)
		{
			return _matchers.All(m => m.IsMatch(request));
		}

		public override string ToString()
		{
			return string.Join(" ", _matchers.Select(m => m.ToString()).ToArray());
		}
	}
}