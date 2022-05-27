#nullable enable
using System.Net.Http.Headers;

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

    private readonly IList<KeyValuePair<string, IEnumerable<string?>>> _headers;

    public HttpHeaderBehavior(IEnumerable<KeyValuePair<string, IEnumerable<string?>>> headers)
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _headers = headers?.ToList() ?? throw new ArgumentNullException(nameof(headers));
    }

    public Task HandleAsync(MockHttpRequestContext requestContext, HttpResponseMessage responseMessage, ResponseHandlerDelegate next, CancellationToken cancellationToken)
    {
        // ReSharper disable once UseDeconstruction
        foreach (KeyValuePair<string, IEnumerable<string?>> header in _headers)
        {
            Add(header, responseMessage);
        }

        return next(requestContext, responseMessage, cancellationToken);
    }

    /// <summary>
    /// When adding header, we have to differentiate between message headers and content headers and prevent adding the header to both.
    /// We also have to prevent adding values to an existing header that only allows a single value.
    /// </summary>
    private static void Add(KeyValuePair<string, IEnumerable<string?>> header, HttpResponseMessage responseMessage)
    {
        // Special case handling of headers which only allow single values.
        if (HeadersWithSingleValueOnly.Contains(header.Key))
        {
            if (responseMessage.Content?.Headers.TryGetValues(header.Key, out _) == true)
            {
                responseMessage.Content.Headers.Remove(header.Key);
            }
            if (responseMessage.Headers.TryGetValues(header.Key, out _))
            {
                responseMessage.Headers.Remove(header.Key);
            }
        }

        // Try to add as message header first, if that fails, add as content header.
        if (!TryAdd(responseMessage.Headers, header.Key, header.Value))
        {
            responseMessage.Content?.Headers.Add(header.Key, header.Value);
        }
    }

    private static bool TryAdd(HttpHeaders? headers, string name, IEnumerable<string?> values)
    {
        if (headers is null)
        {
            return false;
        }

        try
        {
            headers.Add(name, values);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
#nullable restore
