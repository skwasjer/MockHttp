using System.Text;
using MockHttp.Language.Flow.Response;
using MockHttp.Language.Response;
using Newtonsoft.Json;

namespace MockHttp.Json.Newtonsoft;

/// <summary>
/// Response builder extensions.
/// </summary>
public static class ResponseBuilderExtensions
{
    /// <summary>
    /// Sets the JSON content for the response.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="jsonContent">The object to be returned as JSON.</param>
    /// <param name="encoding">The optional JSON encoding.</param>
    /// <param name="serializerSettings">The optional JSON serializer settings. When null uses the default serializer settings.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is <see langword="null" />.</exception>
    public static IWithContentResult JsonBody<T>
    (
        this IWithContent builder,
        T jsonContent,
        Encoding? encoding = null,
        JsonSerializerSettings? serializerSettings = null
    )
    {
        return builder.JsonBody(() => jsonContent, encoding, serializerSettings);
    }

    /// <summary>
    /// Sets the JSON content for the response using a factory returning a new instance of <typeparamref name="T"/> on each invocation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="jsonContentFactory">The factory which creates an object to be returned as JSON.</param>
    /// <param name="encoding">The optional JSON encoding.</param>
    /// <param name="serializerSettings">The optional JSON serializer settings. When null uses the default serializer settings.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="jsonContentFactory" /> is <see langword="null" />.</exception>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IWithContentResult JsonBody<T>
    (
        this IWithContent builder,
        Func<T> jsonContentFactory,
        Encoding? encoding = null,
        JsonSerializerSettings? serializerSettings = null
    )
    {
        return builder.JsonBody(jsonContentFactory, encoding, new NewtonsoftAdapter(serializerSettings));
    }
}
