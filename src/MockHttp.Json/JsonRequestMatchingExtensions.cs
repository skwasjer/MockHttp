namespace MockHttp.Json;

/// <summary>
/// JSON extensions for <see cref="RequestMatching" />.
/// </summary>
public static class JsonRequestMatchingExtensions
{
    /// <summary>
    /// Matches a request by the specified JSON request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="content">The JSON request body.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching JsonBody<T>(this RequestMatching builder, T content)
    {
        return builder.JsonBody(content, null);
    }

    /// <summary>
    /// Matches a request by the specified JSON request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="body">The JSON request body.</param>
    /// <param name="adapter">The JSON adapter.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching JsonBody<T>(this RequestMatching builder, T body, IJsonAdapter? adapter)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new JsonContentMatcher(body, adapter));
    }
}
