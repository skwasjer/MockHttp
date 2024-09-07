using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MockHttp.Fixtures;

public abstract class LoggerFactoryFixture : IAsyncLifetime, IAsyncDisposable
{
    private readonly ServiceProvider _services;

    protected LoggerFactoryFixture(Action<ILoggingBuilder>? configure = null)
    {
        _services = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);

                builder
                    .AddDebug()
#if NET6_0_OR_GREATER
                    .AddSimpleConsole(opts => opts.IncludeScopes = true)
#endif
                    ;

                configure?.Invoke(builder);
            })
            .BuildServiceProvider();

        Factory = _services.GetRequiredService<ILoggerFactory>();
    }

    public ILoggerFactory Factory { get; }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    Task IAsyncLifetime.InitializeAsync()
    {
        return Task.CompletedTask;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return DisposeAsync().AsTask();
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await _services.DisposeAsync();
    }
}
