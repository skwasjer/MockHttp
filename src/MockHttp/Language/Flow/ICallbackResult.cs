using System.ComponentModel;

namespace MockHttp.Language.Flow;

/// <summary>
/// Implements the fluent API.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ICallbackResult<out TResponseResult, out TThrowsResult> : IResponds<TResponseResult>, IThrows<TThrowsResult>, IFluentInterface
    where TResponseResult : IResponseResult
    where TThrowsResult : IThrowsResult
{
}
