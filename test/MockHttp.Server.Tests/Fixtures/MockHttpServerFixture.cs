using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace MockHttp.Fixtures;

public class MockHttpServerFixture : IDisposable, IAsyncLifetime
{
    private readonly Guid _logRequestScope = Guid.NewGuid();
    private readonly LoggerFactoryFixture _loggerFactoryFixture;

    public MockHttpServerFixture()
        : this("http")
    {
    }

    protected MockHttpServerFixture(string scheme)
    {
        _loggerFactoryFixture = new LoggerFactoryFixture();
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
            .Configure(
                builder => builder.Use(
                    async (context, func) =>
                    {
                        ILogger<MockHttpServerFixture>
                            logger = context.RequestServices.GetRequiredService<ILogger<MockHttpServerFixture>>();
                        using IDisposable? scope = logger.BeginScope(_logRequestScope);
                        await func(context);
                    }
                )
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
        await Server.DisposeAsync();
        Handler.Dispose();
        await _loggerFactoryFixture.DisposeAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Server.Dispose();
            Handler.Dispose();
            _loggerFactoryFixture.Dispose();
        }
    }

    private static bool SupportsIpv6()
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        return networkInterfaces.Any(ni => ni.Supports(NetworkInterfaceComponent.IPv6));
    }

    public void LogServerTrace(ITestOutputHelper testOutputHelper)
    {
        foreach (string msg in _loggerFactoryFixture.FakeLogCollector.GetSnapshot()
            .Where(e => e.Scopes.Contains(_logRequestScope))
            .Select(FakeLogRecordSerialization.Serialize))
        {
            testOutputHelper.WriteLine(msg);
        }
    }

    public void Reset()
    {
        Handler.Reset();
        _loggerFactoryFixture.FakeLogCollector.Clear();
    }
}
