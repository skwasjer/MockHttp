using MockHttp.Responses;

namespace MockHttp.Response.Behaviors;

internal sealed class HttpContentBehavior
    : IResponseBehavior
{
    private readonly Func<CancellationToken, Task<HttpContent>> _httpContentFactory;

    public HttpContentBehavior(Func<CancellationToken, Task<HttpContent>> httpContentFactory)
    {
        _httpContentFactory = httpContentFactory ?? throw new ArgumentNullException(nameof(httpContentFactory));
    }

    public async Task HandleAsync(
        MockHttpRequestContext requestContext,
        HttpResponseMessage responseMessage,
        ResponseHandlerDelegate nextHandler,
        CancellationToken cancellationToken
    )
    {
        responseMessage.Content = await _httpContentFactory(cancellationToken).ConfigureAwait(false);
        await nextHandler(requestContext, responseMessage, cancellationToken).ConfigureAwait(false);
    }
}
