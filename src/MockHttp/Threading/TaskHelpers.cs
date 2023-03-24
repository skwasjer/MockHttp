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
}
