#nullable enable
using System.Net.Http.Headers;
using System.Text;
using MockHttp.Responses;

namespace MockHttp;

internal class ResponseBuilder
    : IResponseStrategy,
      IResponseBuilder
{
    internal static readonly MediaTypeHeaderValue DefaultMediaType = new("text/plain");
    internal static readonly Encoding DefaultWebEncoding = Encoding.UTF8;

    private static readonly Dictionary<Type, int> BehaviorPriority = new()
    {
        { typeof(TimeoutBehavior), 0 },
        { typeof(StatusCodeBehavior), 1 },
        { typeof(HttpContentBehavior), 2 },
        { typeof(HttpHeaderBehavior), 3 }
    };

    async Task<HttpResponseMessage> IResponseStrategy.ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
    {
        static Task Seed(MockHttpRequestContext requestContext, HttpResponseMessage responseMessage, CancellationToken cancellationToken) => Task.CompletedTask;

        var response = new HttpResponseMessage();

        await Behaviors
            .ToArray() // clone to array prevent the list from changing while executing.
            .OrderBy(behavior => BehaviorPriority.TryGetValue(behavior.GetType(), out int priority) ? priority : int.MaxValue)
            .Reverse()
            .Aggregate((ResponseHandlerDelegate)Seed,
                (next, pipeline) => (context, message, ct) => pipeline.HandleAsync(context, message, next, ct)
            )(requestContext, response, cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public IList<IResponseBehavior> Behaviors { get; } = new List<IResponseBehavior>();
}

#nullable restore
