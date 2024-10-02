using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace MockHttp.Fixtures;

internal sealed class LoggerFactoryFixture : IAsyncDisposable, IDisposable
{
    private readonly ServiceProvider _services;

    public LoggerFactoryFixture(Action<ILoggingBuilder>? configure = null)
    {
        _services = new ServiceCollection()
            .AddLogging(
                builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Trace);

                    builder
                        .AddFakeLogging()
                        .AddDebug();

                    configure?.Invoke(builder);
                }
            )
            .BuildServiceProvider();

        Factory = _services.GetRequiredService<ILoggerFactory>();
        FakeLogCollector = _services.GetRequiredService<FakeLogCollector>();
    }

    public ILoggerFactory Factory { get; }

    public FakeLogCollector FakeLogCollector { get; }

    public void Dispose()
    {
        _services.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _services.DisposeAsync();
    }
}
