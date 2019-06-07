using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientMock
{
	public interface IMockedHttpRequest //: IVerifies
	{
		/// <summary>Send an HTTP request as an asynchronous operation.</summary>
		/// <param name="request">The HTTP request message to send.</param>
		/// <param name="cancellationToken">The cancellation token to cancel operation.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="request" /> was <see langword="null" />.</exception>
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);

		bool Matches(HttpRequestMessage request);
	}
}
