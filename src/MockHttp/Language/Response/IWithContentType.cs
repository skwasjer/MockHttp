using System.ComponentModel;
using System.Net.Http.Headers;
using MockHttp.Language.Flow.Response;

namespace MockHttp.Language.Response;

/// <summary>
/// Defines the <c>ContentType</c> verb.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IWithContentType
    : IWithResponse,
      IFluentInterface
{
    /// <summary>
    /// Sets the content type for the response.
    /// </summary>
    /// <param name="mediaType">The media type.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaType" /> is <see langword="null" />.</exception>
    public IWithHeadersResult ContentType(MediaTypeHeaderValue mediaType);
}
