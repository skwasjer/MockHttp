using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MockHttp.Responses
{
	/// <summary>
	/// Represents a strategy that produces a mocked response.
	/// </summary>
	internal interface IResponseStrategy
	{
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
	}
}
