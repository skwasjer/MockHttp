#nullable enable
namespace MockHttp.Responses;

internal sealed class HttpHeaderBehavior
    : IResponseBehavior
{
    //https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
    private static readonly ISet<string> HeadersWithSingleValueOnly = new HashSet<string>
    {
        // TODO: expand this list.
        "Age",
        "Authorization",
        "Connection",
        "Content-Length",
        "Content-Location",
        "Content-MD5",
        "Content-Range",
        "Content-Type",
        "Date",
        "ETag",
        "Expires",
        "Host",
        "Last-Modified",
        "Location",
        "Max-Forwards",
        "Retry-After",
        "Server"
    };

    private readonly IList<KeyValuePair<string, IEnumerable<string>>> _headers;

    public HttpHeaderBehavior(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        _headers = headers?.ToList() ?? throw new ArgumentNullException(nameof(headers));
    }

    public Task HandleAsync(MockHttpRequestContext requestContext, HttpResponseMessage responseMessage, ResponseHandlerDelegate next, CancellationToken cancellationToken)
    {
        // ReSharper disable once UseDeconstruction
        foreach (KeyValuePair<string, IEnumerable<string>> header in _headers)
        {
            // Special case handling of headers which only allow single values.
            if (HeadersWithSingleValueOnly.Contains(header.Key))
            {
                responseMessage.Content?.Headers.Remove(header.Key);
            }

            responseMessage.Content?.Headers.Add(header.Key, header.Value);
        }

        return next(requestContext, responseMessage, cancellationToken);
    }
}
#nullable restore
