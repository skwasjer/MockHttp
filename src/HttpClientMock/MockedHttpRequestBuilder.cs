using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpClientMock.Language;

namespace HttpClientMock
{
	internal sealed class MockedHttpRequestBuilder : IMockedHttpRequest, IConfiguredRequest
	{
		private readonly HttpClientMockHandler _handler;
		private Action<HttpRequestMessage, HttpResponseMessage> _callback;
		private string _verifiableBecause;
		private Func<HttpRequestMessage, Task<HttpResponseMessage>> _response;

		public MockedHttpRequestBuilder()
		{
		}

		public MockedHttpRequestBuilder(HttpClientMockHandler handler)
			: this()
		{
			_handler = handler;
		}

		public List<IHttpRequestMatcher> Matchers { get; } = new List<IHttpRequestMatcher>();

		public IConfiguredRequest When(Action<RequestMatching> matching)
		{
			var b = new RequestMatching();
			matching(b);
			Matchers.AddRange(b.Build());
			return this;
		}

		public ICallbackResult Callback(Action<HttpRequestMessage, HttpResponseMessage> callback)
		{
			_callback = callback;
			return this;
		}

		public ICallbackResult Callback(Action callback)
		{
			return Callback(_ => callback());
		}

		public ICallbackResult Callback(Action<HttpRequestMessage> callback)
		{
			return Callback((r, _) => callback(r));
		}

		public IResponseResult RespondWithAsync(Func<HttpResponseMessage> response)
		{
			return RespondWithAsync(_ => response());
		}

		public IResponseResult RespondWithAsync(Func<HttpRequestMessage, HttpResponseMessage> response)
		{
			return RespondWith(request => Task.FromResult(response(request)));
		}

		public IResponseResult RespondWith(Func<HttpRequestMessage, Task<HttpResponseMessage>> response)
		{
			_response = response;
			_handler?.Add(this);
			return this;
		}

		public IResponseResult RespondWith(Func<Task<HttpResponseMessage>> response)
		{
			return RespondWith(_ => response());
		}

		public IThrowsResult Throws(Exception exception)
		{
			RespondWithAsync(_ => throw exception);
			return this;
		}

		public IThrowsResult Throws<TException>()
			where TException : Exception, new()
		{
			RespondWithAsync(_ => throw new TException());
			return this;
		}

		internal bool IsVerifiable { get; private set; }

		public void Verifiable()
		{
			IsVerifiable = true;
		}

		public void Verifiable(string because)
		{
			IsVerifiable = true;
			_verifiableBecause = because;
		}

		public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (_response == null)
			{
				throw new InvalidOperationException("No response configured for http mock.");
			}

			HttpResponseMessage response = await _response(request).ConfigureAwait(false);

			_callback?.Invoke(request, response);

			return response;
		}

		public bool Matches(HttpRequestMessage request)
		{
			return Matchers.All(m => m.IsMatch(request));
		}

		public override string ToString()
		{
			return string.Join(" ", Matchers.Select(m => m.ToString()).ToArray());
		}
	}
}