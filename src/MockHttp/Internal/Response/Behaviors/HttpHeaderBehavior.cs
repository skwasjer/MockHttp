using MockHttp.Http;
using MockHttp.Responses;

namespace MockHttp.Response.Behaviors;

internal sealed class HttpHeaderBehavior
    : IResponseBehavior
{
    //https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
    private static readonly HashSet<string> HeadersWithSingleValueOnly =
    [
        // TODO: expand this list.
        HeaderNames.Age,
        HeaderNames.Authorization,
        HeaderNames.Connection,
        HeaderNames.ContentLength,
        HeaderNames.ContentLocation,
        HeaderNames.ContentMD5,
        HeaderNames.ContentRange,
        HeaderNames.ContentType,
        HeaderNames.Date,
        HeaderNames.ETag,
        HeaderNames.Expires,
        HeaderNames.Host,
        HeaderNames.LastModified,
        HeaderNames.Location,
        HeaderNames.MaxForwards,
        HeaderNames.RetryAfter,
        HeaderNames.Server
    ];

    private readonly IList<KeyValuePair<string, IEnumerable<string?>>> _headers;

    public HttpHeaderBehavior(IEnumerable<KeyValuePair<string, IEnumerable<string?>>> headers)
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _headers = headers?.ToList() ?? throw new ArgumentNullException(nameof(headers));
    }

    public Task HandleAsync(
        MockHttpRequestContext requestContext,
        HttpResponseMessage responseMessage,
        ResponseHandler nextHandler,
        CancellationToken cancellationToken
    )
    {
        // ReSharper disable once UseDeconstruction
        foreach (KeyValuePair<string, IEnumerable<string?>> header in _headers)
        {
            Add(header, responseMessage);
        }

        return nextHandler(requestContext, responseMessage, cancellationToken);
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
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
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
        // Let it throw if not supported.
        if (!responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value))
        {
            responseMessage.Content?.Headers.Add(header.Key, header.Value);
        }
    }
}
