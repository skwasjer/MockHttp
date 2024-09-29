using System.ComponentModel;
using MockHttp.Language.Flow;
using MockHttp.Response;

namespace MockHttp.Language;

/// <summary>
/// Defines the <c>Responds</c> verb and overloads for configuring responses.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IResponds<out TResult> : IFluentInterface
    where TResult : IResponseResult
{
    /// <summary>
    /// Specifies a strategy that returns the response for a request.
    /// </summary>
    /// <param name="responseStrategy">The response strategy that provides the response message to return for given request.</param>
    TResult RespondUsing(IResponseStrategy responseStrategy);
}
