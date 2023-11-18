﻿using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MockHttp.Http;
using MockHttp.Server;

namespace MockHttp;

/// <summary>
/// A mock HTTP server that listens on a specific URL and responds according to a configured <see cref="MockHttpHandler" />.
/// </summary>
public sealed class MockHttpServer : IDisposable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly object _syncLock = new();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly IWebHostBuilder _webHostBuilder;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IWebHost? _host;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Uri _hostUri;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Action<IApplicationBuilder>? _configureAppBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpServer" /> using specified <paramref name="mockHttpHandler" /> and configures it to listen on specified <paramref name="hostUrl" />.
    /// </summary>
    /// <param name="mockHttpHandler">The mock http handler.</param>
    /// <param name="hostUrl">The host URL the mock HTTP server will listen on.</param>
    [Obsolete("Use the overload accepting an System.Uri.")]
    public MockHttpServer(MockHttpHandler mockHttpHandler, string hostUrl)
        : this(mockHttpHandler, GetHostUrl(hostUrl))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpServer" /> using specified <paramref name="mockHttpHandler" /> and configures it to listen on specified <paramref name="hostUri" />.
    /// </summary>
    /// <param name="mockHttpHandler">The mock http handler.</param>
    /// <param name="hostUri">The host URI the mock HTTP server will listen on.</param>
    public MockHttpServer(MockHttpHandler mockHttpHandler, Uri hostUri)
        : this(mockHttpHandler, null, hostUri)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpServer" /> using specified <paramref name="mockHttpHandler" /> and configures it to listen on specified <paramref name="hostUrl" />.
    /// </summary>
    /// <param name="mockHttpHandler">The mock http handler.</param>
    /// <param name="loggerFactory">The logger factory to use to log pipeline requests to.</param>
    /// <param name="hostUrl">The host URL the mock HTTP server will listen on.</param>
    [Obsolete("Use the overload accepting an System.Uri.")]
    public MockHttpServer(MockHttpHandler mockHttpHandler, ILoggerFactory? loggerFactory, string hostUrl)
        : this(mockHttpHandler, loggerFactory, GetHostUrl(hostUrl))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpServer" /> using specified <paramref name="mockHttpHandler" /> and configures it to listen on specified <paramref name="hostUri" />.
    /// </summary>
    /// <param name="mockHttpHandler">The mock http handler.</param>
    /// <param name="loggerFactory">The logger factory to use to log pipeline requests to.</param>
    /// <param name="hostUri">The host URI the mock HTTP server will listen on.</param>
    public MockHttpServer(MockHttpHandler mockHttpHandler, ILoggerFactory? loggerFactory, Uri hostUri)
    {
        Handler = mockHttpHandler ?? throw new ArgumentNullException(nameof(mockHttpHandler));
        _webHostBuilder = CreateWebHostBuilder(loggerFactory);
        _hostUri = new Uri(hostUri, "/"); // Ensure base URL.
        _webHostBuilder.UseUrls(_hostUri.ToString());
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _host?.Dispose();
        _host = null;
    }

    /// <summary>
    /// Gets the <see cref="MockHttpHandler" /> that is simulating middleware.
    /// </summary>
    public MockHttpHandler Handler { get; }

    /// <summary>
    /// Gets the host URL the mock HTTP server will listen on.
    /// </summary>
    [Obsolete("Use the HostUri instead.")]
#pragma warning disable CA1056 // URI-like properties should not be strings
    public string HostUrl => HostUri.ToString().TrimEnd('/');
#pragma warning restore CA1056 // URI-like properties should not be strings

    /// <summary>
    /// Gets the host URI the mock HTTP server will listen on.
    /// </summary>
    public Uri HostUri
    {
        get
        {
            lock (_syncLock)
            {
                string? url = _host?.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses.First();
                return url is null
                    ? _hostUri
                    : new Uri(url);
            }
        }
    }

    /// <summary>
    /// Gets whether the host is started.
    /// </summary>
    public bool IsStarted => _host != null;

    /// <summary>
    /// Starts listening on the configured addresses.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_host != null)
        {
            throw new InvalidOperationException($"{nameof(MockHttpServer)} already running.");
        }

        lock (_syncLock)
        {
            if (_host != null)
            {
                return Task.CompletedTask;
            }

            _host = _webHostBuilder.Build();
            return _host.StartAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Attempt to gracefully stop the mock HTTP server.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_host == null)
        {
            throw new InvalidOperationException($"{nameof(MockHttpServer)} not running.");
        }

        lock (_syncLock)
        {
            if (_host == null)
            {
                return Task.CompletedTask;
            }

            // Make local copy, so we can null it before disposing.
            IWebHost host = _host;
            _host = null;
            try
            {
                return host.StopAsync(cancellationToken);
            }
            finally
            {
                host.Dispose();
            }
        }
    }

    /// <summary>
    /// Creates a <see cref="HttpClient" /> with base address set to URL the server is hosting on.
    /// </summary>
    public HttpClient CreateClient()
    {
        return new HttpClient { BaseAddress = HostUri };
    }

    private IWebHostBuilder CreateWebHostBuilder(ILoggerFactory? loggerFactory)
    {
        return new WebHostBuilder()
            .ConfigureServices(services =>
            {
#pragma warning disable S4792 // Justification: not a security issue, we are injection a null impl. if none is provided.
                services.Replace(ServiceDescriptor.Singleton(loggerFactory ?? new NullLoggerFactory()));
#pragma warning restore S4792
                services.AddSingleton(Handler);
                services.AddTransient<ServerRequestHandler>();
            })
            .UseKestrel(options => options.AddServerHeader = false)
            .CaptureStartupErrors(false)
            .SuppressStatusMessages(true)
            .Configure(applicationBuilder =>
            {
                _configureAppBuilder?.Invoke(applicationBuilder);

                AddMockHttpServerHeader(applicationBuilder);

                ServerRequestHandler serverRequestHandler = applicationBuilder.ApplicationServices.GetRequiredService<ServerRequestHandler>();
                applicationBuilder.Use(serverRequestHandler.HandleAsync);
            });
    }

    internal void Configure(Action<IApplicationBuilder> configureAppBuilder)
    {
        _configureAppBuilder = configureAppBuilder;
    }

    private static Uri GetHostUrl(string hostUrl)
    {
        if (hostUrl is null)
        {
            throw new ArgumentNullException(nameof(hostUrl));
        }

        return Uri.TryCreate(hostUrl, UriKind.Absolute, out Uri? uri)
            ? uri
            : throw new ArgumentException(Resources.Error_HostUrlIsNotValid, nameof(hostUrl));
    }

    private void AddMockHttpServerHeader(IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.TryGetValue(HeaderNames.Server, out _))
                {
                    context.Response.Headers[HeaderNames.Server] = GetType().ToString();
                }

                return Task.CompletedTask;
            });
            await next().ConfigureAwait(false);
        });
    }
}
