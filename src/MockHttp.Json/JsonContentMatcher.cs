using MockHttp.Json.Extensions;
using MockHttp.Json.SystemTextJson;
using MockHttp.Matchers;
using MockHttp.Response;

namespace MockHttp.Json;

internal sealed class JsonContentMatcher : IAsyncHttpRequestMatcher
{
    private readonly object? _jsonContentAsObject;
    private readonly IJsonAdapter? _adapter;
    private readonly IEqualityComparer<string> _jsonComparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentMatcher" /> class using specified raw <paramref name="jsonContentAsObject" />.
    /// </summary>
    /// <param name="jsonContentAsObject">The request content to match.</param>
    /// <param name="adapter">The JSON serializer adapter.</param>
    public JsonContentMatcher(object? jsonContentAsObject, IJsonAdapter? adapter = null)
        : this(jsonContentAsObject, adapter, new SystemTextJsonEqualityComparer(null))
    {
    }

    internal JsonContentMatcher(object? jsonContentAsObject, IJsonAdapter? adapter, IEqualityComparer<string> jsonComparer)
    {
        _jsonContentAsObject = jsonContentAsObject;
        _adapter = adapter;
        _jsonComparer = jsonComparer;
    }

    public async Task<bool> IsMatchAsync(MockHttpRequestContext requestContext)
    {
        string actualJsonContent = string.Empty;
        if (requestContext.Request.Content is not null && requestContext.Request.Content.Headers.ContentLength > 0)
        {
            // Use of ReadAsStringAsync() will use internal buffer, so we can re-enter this method multiple times.
            // In comparison, ReadAsStream() will return the underlying stream which can only be read once.
            actualJsonContent = await requestContext.Request.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        IJsonAdapter adapter = _adapter ?? requestContext.GetAdapter();
        string expectedJson = adapter.Serialize(_jsonContentAsObject);
        return _jsonComparer.Equals(actualJsonContent, expectedJson);
    }

    public bool IsExclusive => true;
}
