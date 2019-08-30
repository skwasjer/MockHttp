using System.Collections.Generic;
using System.Net.Http;
using MockHttp.Matchers;

namespace MockHttp
{
	/// <summary>
	/// Represents an invoked HTTP request that was matched.
	/// </summary>
	public interface IInvokedHttpRequest
	{
		/// <summary>
		/// Gets the HTTP request message.
		/// </summary>
		HttpRequestMessage Request { get; }

		/// <summary>
		/// Gets the matchers that matched the request.
		/// </summary>
		IReadOnlyCollection<IAsyncHttpRequestMatcher> Matchers { get; }
	}
}