using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace MockHttp.Threading;

internal static class TaskHelpers
{
    public static T RunSync<T>(Func<Task<T>> action, TimeSpan timeout)
    {
        Task<T>? task = null;

        RunSync(() =>
            {
                task = action();
                return (Task)task;
            },
            timeout);

        return task is null ? default! : task.Result;
    }

    public static void RunSync(Func<Task> action, TimeSpan timeout)
    {
        if (SynchronizationContext.Current is null)
        {
            RunSyncAndWait(action, timeout);
        }
        else
        {
            RunSyncAndWait(() => Task.Factory.StartNew(action,
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskScheduler.Default
                    )
                    .Unwrap(),
                timeout);
        }
    }

    private static void RunSyncAndWait(Func<Task> action, TimeSpan timeout)
    {
        try
        {
            action().Wait(timeout);
        }
        catch (AggregateException ex)
        {
            AggregateException flattened = ex.Flatten();
            if (flattened.InnerExceptions.Count == 1)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>
    /// A high res delay, which runs SYNCHRONOUSLY and hence is blocking the current thread.
    /// The delay is actually implemented using a spin loop to ensure high precision. Should
    /// NOT be used in production code, only for creating and verifying mocks/stubs with MockHttp.
    /// </summary>
    /// <param name="delay">The delay time.</param>
    /// <param name="cancellationToken">The cancellation token to abort the delay.</param>
    /// <returns>A task that can be awaited to finish</returns>
    internal static Task HighResDelay(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // Task.Delay(0) resolution is around 15ms.
        // So we use a spin loop instead to have more accurate simulated delays.
        while (true)
        {
            Thread.SpinWait(10);
            if (sw.Elapsed > delay)
            {
                return Task.CompletedTask;
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
