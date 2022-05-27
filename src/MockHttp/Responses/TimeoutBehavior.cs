#nullable enable
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
        // It is somewhat unintuitive to throw TaskCanceledException but this is what HttpClient does atm,
        // so we simulate same behavior.
        // https://github.com/dotnet/corefx/issues/20296
        return Task.Delay(_timeoutAfter, cancellationToken)
            .ContinueWith(_ =>
                {
                    var tcs = new TaskCompletionSource<HttpResponseMessage>();
                    tcs.TrySetCanceled();
                    return tcs.Task;
                },
                TaskScheduler.Current)
            .Unwrap();
    }
}
#nullable restore
