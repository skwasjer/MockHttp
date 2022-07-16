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
public sealed class MockHttpServer : IDisposable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly object _syncLock = new();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly IWebHostBuilder _webHostBuilder;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IWebHost? _host;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string _hostUrl;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Action<IApplicationBuilder>? _configureAppBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpServer" /> using specified <paramref name="mockHttpHandler" /> and configures it to listen on specified <paramref name="hostUrl" />.
    /// </summary>
    /// <param name="mockHttpHandler">The mock http handler.</param>
    /// <param name="hostUrl">The host URL the mock HTTP server will listen on.</param>
    public MockHttpServer(MockHttpHandler mockHttpHandler, string hostUrl)
        : this(mockHttpHandler, null, hostUrl)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpServer" /> using specified <paramref name="mockHttpHandler" /> and configures it to listen on specified <paramref name="hostUrl" />.
    /// </summary>
    /// <param name="mockHttpHandler">The mock http handler.</param>
    /// <param name="loggerFactory">The logger factory to use to log pipeline requests to.</param>
    /// <param name="hostUrl">The host URL the mock HTTP server will listen on.</param>
    public MockHttpServer(MockHttpHandler mockHttpHandler, ILoggerFactory? loggerFactory, string hostUrl)
    {
        Handler = mockHttpHandler ?? throw new ArgumentNullException(nameof(mockHttpHandler));
        _webHostBuilder = CreateWebHostBuilder(loggerFactory);
        _hostUrl = GetHostUrl(hostUrl);
        _webHostBuilder.UseUrls(_hostUrl);
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
    public string HostUrl
    {
        get
        {
            lock (_syncLock)
            {
                return _host?.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses.First() ?? _hostUrl;
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
        return new HttpClient { BaseAddress = new Uri(HostUrl) };
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

    private static string GetHostUrl(string hostUrl)
    {
        if (hostUrl is null)
        {
            throw new ArgumentNullException(nameof(hostUrl));
        }

        if (!Uri.TryCreate(hostUrl, UriKind.Absolute, out Uri? uri))
        {
            throw new ArgumentException(Resources.Error_HostUrlIsNotValid, nameof(hostUrl));
        }

        // Ensure we have a proper host URL without path/query.
        return $"{uri.Scheme}://{uri.Host}:{uri.Port}";
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
