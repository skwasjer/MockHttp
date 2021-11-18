using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace MockHttp.Server;

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
        try
        {
            httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogRequestMessage(httpContext, Resources.Error_VerifyMockSetup);

            httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = Resources.Error_VerifyMockSetup,
                Content = new StringContent(Resources.Error_VerifyMockSetup + Environment.NewLine + ex, Encoding.UTF8, "text/plain")
            };
        }
        finally
        {
            LogRequestMessage(httpContext, Resources.Debug_RequestHandled);
        }

        // Dispose message when response is done.
        response.RegisterForDispose(httpResponseMessage);
        cancellationToken.ThrowIfCancellationRequested();

        IHttpResponseFeature responseFeature = httpContext.Features.Get<IHttpResponseFeature>();
        IHttpResponseBodyFeature responseBodyFeature = httpContext.Features.Get<IHttpResponseBodyFeature>();
        await httpResponseMessage.MapToFeatureAsync(responseFeature, responseBodyFeature, cancellationToken).ConfigureAwait(false);
    }

    private void LogRequestMessage(HttpContext httpContext, string message, LogLevel logLevel = LogLevel.Debug, Exception ex = null)
    {
        string formattedMessage = Resources.RequestLogMessage + message;
#pragma warning disable CA2254 // Template should be a static expression
        _logger.Log(logLevel, ex, formattedMessage, httpContext.Connection.Id, httpContext.TraceIdentifier);
#pragma warning restore CA2254 // Template should be a static expression
    }
}
