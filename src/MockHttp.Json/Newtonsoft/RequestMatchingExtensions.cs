using Newtonsoft.Json;

namespace MockHttp.Json.Newtonsoft;

/// <summary>
/// Newtonsoft JSON extensions for <see cref="RequestMatching" />.
/// </summary>
public static class RequestMatchingExtensions
{
    /// <summary>
    /// Matches a request by request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="content">The JSON request content.</param>
    /// <param name="serializerSettings">The serializer settings.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching JsonContent<T>(this RequestMatching builder, T content, JsonSerializerSettings? serializerSettings)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.JsonContent(content, new NewtonsoftAdapter(serializerSettings));
    }
}
