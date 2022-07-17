using System.Net;

namespace MockHttp.Responses;

internal sealed class StatusCodeBehavior
    : IResponseBehavior
{
    private readonly HttpStatusCode _statusCode;
    private readonly string? _reasonPhrase;

    public StatusCodeBehavior(HttpStatusCode statusCode, string? reasonPhrase)
    {
        if ((int)statusCode < 100)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode));
        }

        _statusCode = statusCode;
        _reasonPhrase = reasonPhrase;
    }

    public Task HandleAsync(MockHttpRequestContext requestContext, HttpResponseMessage responseMessage, ResponseHandlerDelegate next, CancellationToken cancellationToken)
    {
        responseMessage.StatusCode = _statusCode;
        if (_reasonPhrase is not null)
        {
            responseMessage.ReasonPhrase = _reasonPhrase;
        }

        return next(requestContext, responseMessage, cancellationToken);
    }
}
