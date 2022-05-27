using System.ComponentModel;
using MockHttp.Language.Flow.Response;

namespace MockHttp.Language.Response;

/// <summary>
/// Defines the <c>Body</c> verb.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IWithContent
    : IWithResponse,
      IFluentInterface
{
    /// <summary>
    /// Sets the content for the response using a factory returning a new <see cref="HttpContent" /> on each invocation.
    /// </summary>
    /// <param name="httpContentFactory">The factory returning a new instance of <see cref="HttpContent" /> on each invocation.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContentFactory" /> is <see langword="null" />.</exception>
    IWithContentResult Body(Func<Task<HttpContent>> httpContentFactory);
}
