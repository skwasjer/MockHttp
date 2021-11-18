using System.ComponentModel;
using MockHttp.Language.Flow;

namespace MockHttp.Language;

/// <summary>
/// Defines the <c>Callback</c> verb and overloads for callbacks.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ICallback<out TResponseResult, out TThrowsResult> : IFluentInterface
	where TResponseResult : IResponseResult
	where TThrowsResult : IThrowsResult
{
	/// <summary>
	/// Specifies a callback to invoke when the request is sent.
	/// </summary>
	/// <param name="callback">The callback to invoke.</param>
	ICallbackResult<TResponseResult, TThrowsResult> Callback(Action callback);

	/// <summary>
	/// Specifies a callback to invoke when the request is sent.
	/// </summary>
	/// <param name="callback">The callback to invoke.</param>
	ICallbackResult<TResponseResult, TThrowsResult> Callback(Action<HttpRequestMessage> callback);
}