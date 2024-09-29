using MockHttp.Http;
using MockHttp.Response;

namespace MockHttp.Language.Flow.Response;

internal sealed class EnsureHttpContentBehavior
    : IResponseBehavior
{
    public Task HandleAsync(
        MockHttpRequestContext requestContext,
        HttpResponseMessage responseMessage,
        ResponseHandler nextHandler,
        CancellationToken cancellationToken
    )
    {
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        responseMessage.Content ??= new EmptyContent();
        return nextHandler(requestContext, responseMessage, cancellationToken);
    }
}
