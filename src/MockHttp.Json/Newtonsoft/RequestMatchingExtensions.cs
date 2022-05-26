using Newtonsoft.Json;

namespace MockHttp.Json.Newtonsoft;

/// <summary>
/// Newtonsoft JSON extensions for <see cref="RequestMatching" />.
/// </summary>
public static class RequestMatchingExtensions
{
    /// <summary>
    /// Matches a request by the specified JSON request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="body">The JSON request body.</param>
    /// <param name="serializerSettings">The serializer settings.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching JsonBody<T>(this RequestMatching builder, T body, JsonSerializerSettings? serializerSettings)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.JsonBody(body, new NewtonsoftAdapter(serializerSettings));
    }
}
