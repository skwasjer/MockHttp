using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientMock.Language.Flow
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal sealed class HttpRequestSetupPhrase : IConfiguredRequest, IFluentInterface
	{
		private readonly HttpCall _setup;

		public HttpRequestSetupPhrase(HttpCall setup)
		{
			_setup = setup ?? throw new ArgumentNullException(nameof(setup));
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
			_setup.SetResponse(response);
			return this;
		}

		public IResponseResult RespondWith(Func<Task<HttpResponseMessage>> response)
		{
			return RespondWith(_ => response());
		}

		public void Verifiable()
		{
			_setup.SetVerifiable(null);
		}

		public void Verifiable(string because)
		{
			if (because == null)
			{
				throw new ArgumentNullException(nameof(because));
			}

			_setup.SetVerifiable(because);
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
			_setup.SetCallback(callback);
			return this;
		}

		public ICallbackResult Callback(Action callback)
		{
			return Callback(_ => callback());
		}
	}
}