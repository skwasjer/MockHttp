using System.ComponentModel;
using MockHttp.Language.Flow.Response;

namespace MockHttp.Language.Response;

/// <summary>
/// Defines the <c>Headers</c> verb.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IWithHeaders
    : IWithResponse,
      IFluentInterface
{
    /// <summary>
    /// Adds HTTP headers.
    /// </summary>
    /// <param name="headers">The headers.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="headers" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="headers" /> is empty.</exception>
    IWithHeadersResult Headers(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers);
}
