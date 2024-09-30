using MockHttp.Response;

namespace MockHttp.Matchers;

/// <summary>
/// Matches a request by HTTP method.
/// </summary>
internal sealed class HttpMethodMatcher : ValueMatcher<HttpMethod>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpMethodMatcher" /> class using specified <paramref name="method" />.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    public HttpMethodMatcher(HttpMethod method)
        : base(method)
    {
        if (method is null)
        {
            throw new ArgumentNullException(nameof(method));
        }
    }

    /// <inheritdoc />
    public override bool IsMatch(MockHttpRequestContext requestContext)
    {
        if (requestContext is null)
        {
            throw new ArgumentNullException(nameof(requestContext));
        }

        return requestContext.Request.Method == Value;
    }

    /// <inheritdoc />
    public override bool IsExclusive => true;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Method: {Value.Method}";
    }
}
