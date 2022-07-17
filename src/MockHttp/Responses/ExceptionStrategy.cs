namespace MockHttp.Responses;

internal sealed class ExceptionStrategy : IResponseStrategy
{
    private readonly Func<Exception?> _exceptionFactory;

    public ExceptionStrategy(Func<Exception> exceptionFactory)
    {
        _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
    }

    public Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
    {
        throw _exceptionFactory() ?? new HttpMockException("The configured exception cannot be null.");
    }
}
