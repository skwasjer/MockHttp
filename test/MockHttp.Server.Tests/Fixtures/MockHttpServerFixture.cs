using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Builder;
using Xunit.Abstractions;

namespace MockHttp.Fixtures;

public class MockHttpServerFixture : IDisposable, IAsyncLifetime
{
    private readonly CapturingLoggerFactoryFixture _loggerFactoryFixture;
    private CapturingLoggerFactoryFixture.LogContext? _loggerCtx;

    public MockHttpServerFixture()
        : this("http")
    {
    }

    protected MockHttpServerFixture(string scheme)
    {
        _loggerFactoryFixture = new CapturingLoggerFactoryFixture();
        Handler = new MockHttpHandler();
        Server = new MockHttpServer(
            Handler,
            _loggerFactoryFixture.Factory,
            new Uri(
                SupportsIpv6()
                    ? $"{scheme}://[::1]:0"
                    : $"{scheme}://127.0.0.1:0"
            )
        );
        Server
            .Configure(builder => builder
                .Use((_, next) =>
                {
                    _loggerCtx ??= CapturingLoggerFactoryFixture.CreateContext();
                    return next();
                })
            );
    }

    public MockHttpHandler Handler { get; }

    public MockHttpServer Server { get; }

    public Task InitializeAsync()
    {
        return Server.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _loggerFactoryFixture.DisposeAsync();
        await Server.DisposeAsync();
    }

    public void Dispose()
    {
        _loggerCtx?.Dispose();
        Server.Dispose();
        Handler.Dispose();
        GC.SuppressFinalize(this);
    }

    private static bool SupportsIpv6()
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        return networkInterfaces.Any(ni => ni.Supports(NetworkInterfaceComponent.IPv6));
    }

    // ReSharper disable once MemberCanBeMadeStatic.Global
    public void LogServerTrace(ITestOutputHelper testOutputHelper)
    {
        if (_loggerCtx is null)
        {
            return;
        }

        foreach (string msg in _loggerCtx.Events)
        {
            testOutputHelper.WriteLine(msg);
        }
    }

    public void Reset()
    {
        Handler.Reset();
        _loggerCtx = null;
    }
}
