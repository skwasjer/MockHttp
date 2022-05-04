#nullable enable
using System.Net;

namespace MockHttp.Responses;

internal sealed class StatusCodeBehavior
    : IResponseBehavior
{
    private readonly HttpStatusCode _statusCode;

    public StatusCodeBehavior(HttpStatusCode statusCode)
    {
        if ((int)statusCode < 100)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode));
        }

        _statusCode = statusCode;
    }

    public Task HandleAsync(MockHttpRequestContext requestContext, HttpResponseMessage responseMessage, ResponseHandlerDelegate next, CancellationToken cancellationToken)
    {
        responseMessage.StatusCode = _statusCode;
        return next(requestContext, responseMessage, cancellationToken);
    }
}
#nullable restore
