using System.Text.Json;

namespace MockHttp.Json.SystemTextJson;

/// <summary>
/// System.Text.JSON extensions for <see cref="RequestMatching" />.
/// </summary>
public static class RequestMatchingExtensions
{
    /// <summary>
    /// Matches a request by the specified JSON request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="body">The JSON request body.</param>
    /// <param name="serializerOptions">The serializer options.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching JsonBody<T>(this RequestMatching builder, T body, JsonSerializerOptions? serializerOptions)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.JsonBody(body, new SystemTextJsonAdapter(serializerOptions));
    }
}
