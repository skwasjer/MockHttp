using System.Runtime.CompilerServices;
using MockHttp.Threading;

namespace MockHttp;

/// <summary>
/// Defines different types of latencies to simulate a slow network.
/// </summary>
public class NetworkLatency
{
    private static readonly Random Random = new(DateTime.UtcNow.Ticks.GetHashCode());

    private readonly Func<TimeSpan> _factory;
    private readonly string _name;

    static NetworkLatency()
    {
        // Warmup so that actual simulated latency is more accurate.
#pragma warning disable CA5394 // Do not use insecure randomness - justification: not used in secure context
        Random.Next();
#pragma warning restore CA5394 // Do not use insecure randomness
    }

    private NetworkLatency(Func<TimeSpan> factory, string name)
    {
        _factory = factory;
        _name = name;
    }

    /// <summary>
    /// Configures 2G network latency (300ms to 1200ms).
    /// </summary>
    public static NetworkLatency TwoG()
    {
        return Between(300, 1200, nameof(TwoG));
    }

    /// <summary>
    /// Configures 3G network latency (100ms to 600ms).
    /// </summary>
    public static NetworkLatency ThreeG()
    {
        return Between(100, 600, nameof(ThreeG));
    }

    /// <summary>
    /// Configures 4G network latency (30ms to 50ms).
    /// </summary>
    public static NetworkLatency FourG()
    {
        return Between(30, 50, nameof(FourG));
    }

    /// <summary>
    /// Configures 5G network latency (5ms to 10ms).
    /// </summary>
    public static NetworkLatency FiveG()
    {
        return Between(5, 10, nameof(FiveG));
    }

    /// <summary>
    /// Configures a latency to be around <paramref name="latency" />.
    /// </summary>
    /// <param name="latency">The latency.</param>
    public static NetworkLatency Around(TimeSpan latency)
    {
        int latencyInMs = Convert.ToInt32(latency.TotalMilliseconds);
        return Between(latencyInMs, latencyInMs, $"{nameof(Around)}({latency})");
    }

    /// <summary>
    /// Configures a latency to be around <paramref name="latencyInMs" />.
    /// </summary>
    /// <param name="latencyInMs">The latency in milliseconds.</param>
    public static NetworkLatency Around(int latencyInMs)
    {
        return Between(latencyInMs, latencyInMs, $"{nameof(Around)}({latencyInMs})");
    }

    /// <summary>
    /// Configures a random latency between <paramref name="min" /> and <paramref name="max" />.
    /// </summary>
    /// <param name="min">The minimum latency.</param>
    /// <param name="max">The maximum latency.</param>
    public static NetworkLatency Between(TimeSpan min, TimeSpan max)
    {
        return Between(Convert.ToInt32(min.TotalMilliseconds), Convert.ToInt32(max.TotalMilliseconds), $"{nameof(Between)}({min}, {max})");
    }

    /// <summary>
    /// Configures a random latency between <paramref name="minMs" /> and <paramref name="maxMs" />.
    /// </summary>
    /// <param name="minMs">The minimum latency in milliseconds.</param>
    /// <param name="maxMs">The maximum latency in milliseconds.</param>
    public static NetworkLatency Between(int minMs, int maxMs)
    {
        return Between(minMs, maxMs, $"{nameof(Between)}({minMs}, {maxMs})");
    }

    private static NetworkLatency Between(int minMs, int maxMs, string name)
    {
        if (minMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minMs));
        }

        if (minMs > maxMs)
        {
            throw new ArgumentOutOfRangeException(nameof(maxMs));
        }

        return new NetworkLatency(() =>
            {
#pragma warning disable CA5394 // Do not use insecure randomness - justification: not used in secure context
                double randomLatency = Random.Next(minMs, maxMs);
#pragma warning restore CA5394
                return TimeSpan.FromMilliseconds(randomLatency);
            },
            name);
    }

    /// <summary>
    /// Simulates a random latency by introducing a delay before executing the task.
    /// </summary>
    /// <param name="task">The task to execute after the delay.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The random latency generated for this simulation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal async Task<TimeSpan> SimulateAsync(Func<Task> task, CancellationToken cancellationToken = default)
    {
        TimeSpan latency = _factory();
        await HighResDelay.WaitAsync(latency, cancellationToken: cancellationToken).ConfigureAwait(false);
        await task().ConfigureAwait(false);
        return latency;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{GetType().Name}.{_name}";
    }
}
