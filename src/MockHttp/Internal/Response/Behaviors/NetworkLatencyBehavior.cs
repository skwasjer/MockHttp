using MockHttp.Responses;

namespace MockHttp.Response.Behaviors;

internal sealed class NetworkLatencyBehavior
    : IResponseBehavior
{
    private readonly NetworkLatency _networkLatency;

    public NetworkLatencyBehavior(NetworkLatency networkLatency)
    {
        _networkLatency = networkLatency ?? throw new ArgumentNullException(nameof(networkLatency));
    }

    public Task HandleAsync(
        MockHttpRequestContext requestContext,
        HttpResponseMessage responseMessage,
        ResponseHandler nextHandler,
        CancellationToken cancellationToken
    )
    {
        return _networkLatency.SimulateAsync(() => nextHandler(requestContext, responseMessage, cancellationToken), cancellationToken);
    }
}
