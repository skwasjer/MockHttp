using System.ComponentModel;
using System.Net.Http.MockHttp.Language.Flow;
using System.Threading.Tasks;

namespace System.Net.Http.MockHttp.Language
{
	/// <summary>
	/// Defines the <c>Responds</c> verb and overloads for configuring responses.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IResponds : IFluentInterface
	{
		/// <summary>
		/// Specifies a function that returns the response for a request.
		/// </summary>
		/// <param name="response">The function returning an awaitable that when completed provides the response message to return for a request.</param>
		IResponseResult RespondWith(Func<Task<HttpResponseMessage>> response);

		/// <summary>
		/// Specifies a function that returns the response for a request.
		/// </summary>
		/// <param name="response">The function returning an awaitable that when completed provides the response message to return for given request.</param>
		IResponseResult RespondWith(Func<HttpRequestMessage, Task<HttpResponseMessage>> response);
	}
}