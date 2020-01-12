using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MockHttp.Server
{
	internal class RequestHandler : DelegatingHandler
	{
		public RequestHandler(HttpMessageHandler handler)
		{
			InnerHandler = handler;
		}

		public Task<HttpResponseMessage> HandleAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return SendAsync(request, cancellationToken);
		}
	}
}
