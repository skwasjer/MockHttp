using System.Diagnostics;
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
public sealed class MockHttpServer
    : IDisposable,
      IAsyncDisposable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Uri _hostUri;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly SemaphoreSlim _lock = new(1);
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly IWebHostBuilder _webHostBuilder;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Action<IApplicationBuilder>? _configureAppBuilder;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _disposed;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IWebHost? _host;

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
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await StopAsync().ConfigureAwait(false);
        _lock.Dispose();
        _disposed = true;
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
            CheckDisposed();

            _lock.Wait();
            try
            {
                string? url = _host?.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses.First();
                return url is null
                    ? _hostUri
                    : new Uri(url);
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    /// <summary>
    /// Gets whether the host is started.
    /// </summary>
    public bool IsStarted => !_disposed && _host is not null;

    /// <summary>
    /// Starts listening on the configured addresses.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        CheckDisposed();

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_host is not null)
            {
                return;
            }

            _host = _webHostBuilder.Build();
            await _host.StartAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Attempt to gracefully stop the mock HTTP server.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        CheckDisposed();

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_host is null)
            {
                return;
            }

            await _host.StopAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (_host is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                _host?.Dispose();
            }

            _host = null;
            _lock.Release();
        }
    }

    /// <summary>
    /// Creates a <see cref="HttpClient" /> with base address set to URL the server is hosting on.
    /// </summary>
    public HttpClient CreateClient()
    {
        CheckDisposed();
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

                applicationBuilder.UseMockHttpExceptionHandler();

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

    private void CheckDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
