#nullable enable
namespace MockHttp.Responses;

internal sealed class HttpContentBehavior
    : IResponseBehavior
{
    private readonly Func<Task<HttpContent>> _httpContentFactory;

    public HttpContentBehavior(Func<Task<HttpContent>> httpContentFactory)
    {
        _httpContentFactory = httpContentFactory ?? throw new ArgumentNullException(nameof(httpContentFactory));
    }

    public async Task HandleAsync(MockHttpRequestContext requestContext, HttpResponseMessage responseMessage, ResponseHandlerDelegate next, CancellationToken cancellationToken)
    {
        responseMessage.Content = await _httpContentFactory();
        await next(requestContext, responseMessage, cancellationToken).ConfigureAwait(false);
    }
}
#nullable restore
