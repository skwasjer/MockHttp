﻿namespace MockHttp.Responses;

internal sealed class TimeoutStrategy : IResponseStrategy
{
	private readonly TimeSpan _timeoutAfter;

	public TimeoutStrategy(TimeSpan timeoutAfter)
	{
		if (timeoutAfter.TotalMilliseconds <= -1L || timeoutAfter.TotalMilliseconds > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException(nameof(timeoutAfter));
		}

		_timeoutAfter = timeoutAfter;
	}

	public Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
	{
		// It is somewhat unintuitive to throw TaskCanceledException but this is what HttpClient does atm,
		// so we simulate same behavior.
		// https://github.com/dotnet/corefx/issues/20296
		return Task.Delay(_timeoutAfter, cancellationToken)
			.ContinueWith(_ =>
			{
				var tcs = new TaskCompletionSource<HttpResponseMessage>();
				tcs.TrySetCanceled();
				return tcs.Task;
			}, TaskScheduler.Current)
			.Unwrap();
	}
}