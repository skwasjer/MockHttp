namespace MockHttp.Responses;

internal sealed class TimeoutBehavior
    : IResponseBehavior
{
    private readonly TimeSpan _timeoutAfter;

    public TimeoutBehavior(TimeSpan timeoutAfter)
    {
        if (timeoutAfter.TotalMilliseconds is <= -1L or > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutAfter));
        }

        _timeoutAfter = timeoutAfter;
    }

    public Task HandleAsync(MockHttpRequestContext requestContext, HttpResponseMessage responseMessage, ResponseHandlerDelegate next, CancellationToken cancellationToken)
    {
        // It is somewhat unintuitive to throw TaskCanceledException but this is what HttpClient does,
        // so we simulate same behavior.
        // https://github.com/dotnet/corefx/issues/20296
        // Beginning in .NET 5, the TaskCanceledException's inner exception is a TimeoutException to clarify client timeout.

        return Task.Delay(_timeoutAfter, cancellationToken)
            .ContinueWith(_ =>
                {
                    var tcs = new TaskCompletionSource<HttpResponseMessage>();
#if NET5_0_OR_GREATER
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        tcs.TrySetException(new TaskCanceledException(null, new TimeoutException()));
                    }
                    else
                    {
                        tcs.TrySetCanceled();
                    }
#else
                    tcs.TrySetCanceled();
#endif
                    return tcs.Task;
                },
                TaskScheduler.Current)
            .Unwrap();
    }
}
