#nullable enable
using System.ComponentModel;
using System.Net;
using MockHttp.Language.Flow.Response;

namespace MockHttp.Language.Response;

/// <summary>
/// Defines the <c>StatusCode</c> verb.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IWithStatusCode
    : IWithContent,
      IFluentInterface
{
    /// <summary>
    /// Sets the status code for the response.
    /// </summary>
    /// <param name="statusCode">The status code to return with the response.</param>
    /// <param name="reasonPhrase">The reason phrase which typically is sent by servers together with the status code.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="statusCode" /> is less than 100.</exception>
    IWithStatusCodeResult StatusCode(HttpStatusCode statusCode, string? reasonPhrase = null);
}
#nullable restore
