namespace MockHttp.Responses
{
	internal sealed class ResponseFuncStrategy : IResponseStrategy
	{
		private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responseFunc;

		public ResponseFuncStrategy(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFunc)
		{
			_responseFunc = responseFunc ?? throw new ArgumentNullException(nameof(responseFunc));
		}

		public Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
		{
			return _responseFunc?.Invoke(requestContext.Request, cancellationToken) ?? Task.FromResult<HttpResponseMessage>(null);
		}
	}
}
