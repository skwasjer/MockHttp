using System;
using System.Net.Http;
using System.Threading.Tasks;
using HttpClientMock.Language;

namespace HttpClientMock
{
	internal sealed class HttpRequestSetupPhrase : IConfiguredRequest
	{
		private readonly MockedHttpRequest _mockedHttpRequest;

		public HttpRequestSetupPhrase(MockedHttpRequest mockedHttpRequest)
		{
			_mockedHttpRequest = mockedHttpRequest ?? throw new ArgumentNullException(nameof(mockedHttpRequest));
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
			_mockedHttpRequest.SetResponse(response);
			return this;
		}

		public IResponseResult RespondWith(Func<Task<HttpResponseMessage>> response)
		{
			return RespondWith(_ => response());
		}

		public void Verifiable()
		{
			_mockedHttpRequest.SetVerifiable(null);
		}

		public void Verifiable(string because)
		{
			if (because == null)
			{
				throw new ArgumentNullException(nameof(because));
			}

			_mockedHttpRequest.SetVerifiable(because);
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

		public ICallbackResult Callback(Action<HttpRequestMessage> callback)
		{
			_mockedHttpRequest.SetCallback(callback);
			return this;
		}

		public ICallbackResult Callback(Action callback)
		{
			return Callback(_ => callback());
		}
	}
}