using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace MockHttp.Server
{
	internal class ServerRequestHandler : DelegatingHandler
	{
		public ServerRequestHandler(HttpMessageHandler handler)
		{
			InnerHandler = handler;
		}

#pragma warning disable IDE0060 // Remove unused parameter
		public async Task HandleAsync(HttpContext httpContext, Func<Task> next)
#pragma warning restore IDE0060 // Remove unused parameter
		{
			if (httpContext is null)
			{
				throw new ArgumentNullException(nameof(httpContext));
			}

			CancellationToken cancellationToken = httpContext.RequestAborted;
			HttpResponse response = httpContext.Response;

			try
			{
				using HttpRequestMessage httpRequestMessage = new WrappedHttpRequest(httpContext.Request);
				cancellationToken.ThrowIfCancellationRequested();

				HttpResponseMessage httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
				// Dispose message when response is done.
				response.RegisterForDispose(httpResponseMessage);
				cancellationToken.ThrowIfCancellationRequested();

				IHttpResponseFeature responseFeature = httpContext.Features.Get<IHttpResponseFeature>();
				await httpResponseMessage.MapToFeatureAsync(responseFeature, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw;
			}
		}
	}
}
