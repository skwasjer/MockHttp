using System.ComponentModel;
using MockHttp.Language.Flow;
using MockHttp.Responses;

namespace MockHttp.Language;

/// <summary>
/// Defines the <c>Responds</c> verb and overloads for configuring responses.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IResponds<out TResult> : IFluentInterface
	where TResult : IResponseResult
{
	/// <summary>
	/// Specifies a function that returns the response for a request.
	/// </summary>
	/// <param name="responseFunc">The function returning an awaitable that when completed provides the response message to return for a request.</param>
	TResult Respond(Func<Task<HttpResponseMessage>> responseFunc);

	/// <summary>
	/// Specifies a function that returns the response for a request.
	/// </summary>
	/// <param name="responseFunc">The function returning an awaitable that when completed provides the response message to return for given request.</param>
	TResult Respond(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFunc);

	/// <summary>
	/// Specifies a function that returns the response for a request.
	/// </summary>
	/// <param name="responseFunc">The function returning an awaitable that when completed provides the response message to return for given request.</param>
	TResult Respond(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFunc);

	/// <summary>
	/// Specifies a strategy that returns the response for a request.
	/// </summary>
	/// <param name="responseStrategy">The response strategy that provides the response message to return for given request.</param>
	TResult RespondUsing(IResponseStrategy responseStrategy);
}