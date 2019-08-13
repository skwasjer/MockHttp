using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace MockHttp.Language.Flow
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal sealed class HttpRequestSetupPhrase : IConfiguredRequest, IFluentInterface
	{
		private readonly HttpCall _setup;

		public HttpRequestSetupPhrase(HttpCall setup)
		{
			_setup = setup ?? throw new ArgumentNullException(nameof(setup));
		}

		public ISequenceResponseResult Respond(Func<Task<HttpResponseMessage>> response)
		{
			return Respond(_ => response());
		}

		public ISequenceResponseResult Respond(Func<HttpRequestMessage, Task<HttpResponseMessage>> response)
		{
			_setup.SetResponse(response);
			return this;
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

		public ISequenceThrowsResult Throws(Exception exception)
		{
			Respond(() => throw exception);
			return this;
		}

		public ISequenceThrowsResult Throws<TException>()
			where TException : Exception, new()
		{
			Respond(_ => throw new TException());
			return this;
		}

		public ICallbackResult<ISequenceResponseResult, ISequenceThrowsResult> Callback(Action<HttpRequestMessage> callback)
		{
			_setup.SetCallback(callback);
			return this;
		}

		public ICallbackResult<ISequenceResponseResult, ISequenceThrowsResult> Callback(Action callback)
		{
			return Callback(_ => callback());
		}
	}
}