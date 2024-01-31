using MockHttp.Responses;

namespace MockHttp.Language.Flow.Response;

internal sealed class EnsureHttpContentBehavior
    : IResponseBehavior
{
    public Task HandleAsync
    (
        MockHttpRequestContext requestContext,
        HttpResponseMessage responseMessage,
        ResponseHandlerDelegate next,
        CancellationToken cancellationToken)
    {
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        responseMessage.Content ??= new EmptyContent();
        return next(requestContext, responseMessage, cancellationToken);
    }
}
