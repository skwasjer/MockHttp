using System.Diagnostics;

namespace MockHttp.Threading;

internal static class HighResDelay
{
    /// <summary>
    /// A high res delay, which blocks the current thread.
    /// The delay is actually implemented using a spin loop to ensure high precision. Should
    /// NOT be used in production code, only for creating and verifying mocks/stubs with MockHttp.
    /// </summary>
    /// <param name="delay">The delay time.</param>
    /// <param name="iterations">The number of iterations per spin (lower is gives better accuracy).</param>
    internal static void Wait(TimeSpan delay, int iterations = 100)
    {
        var sw = Stopwatch.StartNew();

        // Task.Delay(0) resolution is around 15ms.
        // So we use a spin loop instead to have more accurate simulated delays.
        while (true)
        {
            if (sw.Elapsed > delay)
            {
                return;
            }

            Thread.SpinWait(iterations);
        }
    }

    /// <summary>
    /// A high res delay, which blocks the current (async) thread.
    /// The delay is actually implemented using a spin loop to ensure high precision. Should
    /// NOT be used in production code, only for creating and verifying mocks/stubs with MockHttp.
    /// </summary>
    /// <param name="delay">The delay time.</param>
    /// <param name="iterations">The number of iterations per spin (lower is gives better accuracy).</param>
    /// <param name="cancellationToken">The cancellation token to abort the delay.</param>
    /// <returns>A task that can be awaited to finish</returns>
    internal static Task WaitAsync(TimeSpan delay, int iterations = 100, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // Task.Delay(0) resolution is around 15ms.
        // So we use a spin loop instead to have more accurate simulated delays.
        while (true)
        {
            if (sw.Elapsed > delay)
            {
                return Task.CompletedTask;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            Thread.SpinWait(iterations);
        }
    }
}
