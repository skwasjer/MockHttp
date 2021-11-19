using MockHttp.Json.Newtonsoft;
using Newtonsoft.Json;

namespace MockHttp.Json;

/// <summary>
/// JSON extensions for <see cref="RequestMatching" />.
/// </summary>
public static class JsonRequestMatchingExtensions
{
    /// <summary>
    /// Matches a request by request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="content">The JSON request content.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching JsonContent<T>(this RequestMatching builder, T content)
    {
        return builder.JsonContent(content, null);
    }

    /// <summary>
    /// Matches a request by request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="content">The JSON request content.</param>
    /// <param name="serializerSettings">The serializer settings.</param>
    /// <returns>The request matching builder instance.</returns>
    // TODO: move Newtonsoft extensions to separate class.
    public static RequestMatching JsonContent<T>(this RequestMatching builder, T content, JsonSerializerSettings? serializerSettings)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(
            new JsonContentMatcher(
                content,
                serializerSettings is null
                    ? null
                    : new NewtonsoftAdapter(serializerSettings)
            )
        );
    }
}
