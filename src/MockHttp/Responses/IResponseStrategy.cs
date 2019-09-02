using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MockHttp.Responses
{
	/// <summary>
	/// Represents a strategy that produces a mocked response.
	/// </summary>
	public interface IResponseStrategy
	{
		/// <summary>
		/// Produces a response message to return for the <paramref name="request"/>.
		/// </summary>
		/// <param name="request">The request message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>An awaitable that when completed provides the response message.</returns>
		Task<HttpResponseMessage> ProduceResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken);
	}
}
