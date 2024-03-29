﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace MockHttp.Server;

internal class ServerRequestHandler : DelegatingHandler
{
    private readonly ILogger<ServerRequestHandler> _logger;

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
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

        _logger.LogRequestMessage(httpContext, Resources.Debug_HandlingRequest);

        CancellationToken cancellationToken = httpContext.RequestAborted;
        HttpResponse response = httpContext.Response;

        using HttpRequestMessage httpRequestMessage = new WrappedHttpRequest(httpContext.Request);
        cancellationToken.ThrowIfCancellationRequested();

        HttpResponseMessage httpResponseMessage;
        try
        {
            httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _logger.LogRequestMessage(httpContext, Resources.Debug_RequestHandled);
        }

        // Dispose message when response is done.
        response.RegisterForDispose(httpResponseMessage);
        cancellationToken.ThrowIfCancellationRequested();

        IHttpResponseFeature? responseFeature = httpContext.Features.Get<IHttpResponseFeature>();
        IHttpResponseBodyFeature? responseBodyFeature = httpContext.Features.Get<IHttpResponseBodyFeature>();
        if (responseFeature is null || responseBodyFeature is null)
        {
            throw new InvalidOperationException(Resources.MissingHttpResponseFeature);
        }

        await httpResponseMessage.MapToFeatureAsync(responseFeature, responseBodyFeature, cancellationToken).ConfigureAwait(false);
    }
}
