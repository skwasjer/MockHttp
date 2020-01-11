using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace MockHttp.Threading
{
	internal static class TaskHelpers
	{
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
				).Unwrap(), timeout);
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
					// ReSharper disable once AssignNullToNotNullAttribute
					ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				}
				else
				{
					throw;
				}
			}
		}
	}
}
