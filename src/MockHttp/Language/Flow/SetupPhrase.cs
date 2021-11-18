using System.ComponentModel;
using MockHttp.Responses;

namespace MockHttp.Language.Flow
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal abstract class SetupPhrase<TResponseResult, TThrowsResult> :
		IResponds<TResponseResult>, IResponseResult,
		IThrows<TThrowsResult>, IThrowsResult,
		IFluentInterface
		where TResponseResult : IResponseResult
		where TThrowsResult : IThrowsResult
	{
		protected SetupPhrase(HttpCall setup)
		{
			Setup = setup ?? throw new ArgumentNullException(nameof(setup));
		}

		protected HttpCall Setup { get; }

		public TResponseResult Respond(Func<Task<HttpResponseMessage>> responseFunc)
		{
			return RespondUsing(new ResponseFuncStrategy((_, __) => responseFunc()));
		}

		public TResponseResult Respond(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFunc)
		{
			return RespondUsing(new ResponseFuncStrategy((r, __) => responseFunc(r)));
		}

		public TResponseResult Respond(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFunc)
		{
			return RespondUsing(new ResponseFuncStrategy(responseFunc));
		}

		public TResponseResult RespondUsing(IResponseStrategy responseStrategy)
		{
			if (responseStrategy is null)
			{
				throw new ArgumentNullException(nameof(responseStrategy));
			}

			Setup.SetResponse(responseStrategy);
			return (TResponseResult)(object)this;
		}

		public TThrowsResult Throws(Exception exception)
		{
			if (exception is null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			RespondUsing(new ExceptionStrategy(() => throw exception));
			return (TThrowsResult)(object)this;
		}

		public TThrowsResult Throws<TException>()
			where TException : Exception, new()
		{
			RespondUsing(new ExceptionStrategy(() => throw new TException()));
			return (TThrowsResult)(object)this;
		}
	}
}
