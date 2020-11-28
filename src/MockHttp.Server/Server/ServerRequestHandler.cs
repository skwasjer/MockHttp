using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace MockHttp.Server
{
	internal class ServerRequestHandler : DelegatingHandler
	{
		private readonly ILogger<ServerRequestHandler> _logger;

		// ReSharper disable once SuggestBaseTypeForParameter
		public ServerRequestHandler(MockHttpHandler mockHttpHandler, ILogger<ServerRequestHandler> logger)
		{
			_logger = logger;
			InnerHandler = mockHttpHandler;
		}

		public async Task HandleAsync(HttpContext httpContext, Func<Task> _)
		{
			if (httpContext is null)
			{
				throw new ArgumentNullException(nameof(httpContext));
			}

			LogRequestMessage(httpContext, Resources.Debug_HandlingRequest);

			CancellationToken cancellationToken = httpContext.RequestAborted;
			HttpResponse response = httpContext.Response;

			using HttpRequestMessage httpRequestMessage = new WrappedHttpRequest(httpContext.Request);
			cancellationToken.ThrowIfCancellationRequested();

			HttpResponseMessage httpResponseMessage;
			IHttpResponseFeature responseFeature = httpContext.Features.Get<IHttpResponseFeature>();
			try
			{
				httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				LogRequestMessage(httpContext, Resources.Error_VerifyMockSetup);

#pragma warning disable CA2000 // Dispose objects before losing scope
				httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					ReasonPhrase = Resources.Error_VerifyMockSetup,
					Content = new StringContent(Resources.Error_VerifyMockSetup + Environment.NewLine + ex, Encoding.UTF8, "text/plain")
				};
#pragma warning restore CA2000 // Dispose objects before losing scope
			}
			finally
			{
				LogRequestMessage(httpContext, Resources.Debug_RequestHandled);
			}

			// Dispose message when response is done.
			response.RegisterForDispose(httpResponseMessage);
			cancellationToken.ThrowIfCancellationRequested();

			await httpResponseMessage.MapToFeatureAsync(responseFeature, cancellationToken).ConfigureAwait(false);
		}

		private void LogRequestMessage(HttpContext httpContext, string message, LogLevel logLevel = LogLevel.Debug, Exception ex = null)
		{
			string formattedMessage = Resources.RequestLogMessage + message;
			_logger.Log(logLevel, ex, formattedMessage, httpContext.Connection.Id, httpContext.TraceIdentifier);
		}
	}
}
