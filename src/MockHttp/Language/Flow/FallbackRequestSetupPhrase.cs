using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MockHttp.Language.Flow
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal sealed class FallbackRequestSetupPhrase : IRespondsThrows, IResponseResult, IThrowsResult, IFluentInterface
	{
		private readonly HttpCall _setup;

		public FallbackRequestSetupPhrase(HttpCall setup)
		{
			_setup = setup ?? throw new ArgumentNullException(nameof(setup));
		}

		public IResponseResult Respond(Func<Task<HttpResponseMessage>> response)
		{
			return Respond((_, __) => response());
		}

		public IResponseResult Respond(Func<HttpRequestMessage, Task<HttpResponseMessage>> response)
		{
			return Respond((r, __) => response(r));
		}

		public IResponseResult Respond(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> response)
		{
			_setup.SetResponse(response);
			return this;
		}

		public IThrowsResult Throws(Exception exception)
		{
			Respond(() => throw exception);
			return this;
		}

		public IThrowsResult Throws<TException>()
			where TException : Exception, new()
		{
			Respond(_ => throw new TException());
			return this;
		}
	}
}