using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientMock
{
	public class MockedHttpRequest : IMockedHttpRequest
	{
		private readonly IMockHttpRequestBuilder _builder;
		private readonly List<IHttpRequestMatcher> _matchers;
		private Func<HttpRequestMessage, Task<HttpResponseMessage>> _response;
		private Action<HttpRequestMessage, HttpResponseMessage> _callback;

		public MockedHttpRequest(IMockHttpRequestBuilder builder)
		{
			_builder = builder ?? throw new ArgumentNullException(nameof(builder));
			_matchers = new List<IHttpRequestMatcher>(1);
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

		public IMockedHttpRequest With(IHttpRequestMatcher matcher)
		{
			_matchers.Add(matcher);
			return this;
		}

		public IMockedHttpRequest Callback(Action callback)
		{
			return Callback(_ => callback());
		}

		public IMockedHttpRequest Callback(Action<HttpRequestMessage> callback)
		{
			return Callback((r, _) => callback(r));
		}

		public IMockedHttpRequest Callback(Action<HttpRequestMessage, HttpResponseMessage> callback)
		{
			_callback = callback;
			return this;
		}

		public IMockedHttpRequest RespondsWith(Func<Task<HttpResponseMessage>> response)
		{
			return RespondsWith(_ => response());
		}

		public IMockedHttpRequest RespondsWith(Func<HttpRequestMessage, Task<HttpResponseMessage>> response)
		{
			_response = response;
			return this;
		}

		public IMockedHttpRequest RespondsWith(Func<HttpRequestMessage, HttpResponseMessage> response)
		{
			return RespondsWith(request => Task.FromResult(response(request)));
		}

		public override string ToString()
		{
			return string.Join(" ", _matchers.Select(m => m.ToString()).ToArray());
		}
	}
}