#nullable enable
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using MockHttp.Http;
using MockHttp.Responses;

namespace MockHttp.Language.Flow.Response;

internal class ResponseBuilder
    : IResponseStrategy,
      IResponseBuilder,
      IWithStatusCodeResult,
      IWithContentResult,
      IWithHeadersResult
{
    internal static readonly Encoding DefaultWebEncoding = Encoding.UTF8;

    private static readonly Func<MockHttpRequestContext, Task<HttpContent>> EmptyHttpContentFactory = _ => Task.FromResult<HttpContent>(new EmptyContent());
    private static readonly Dictionary<Type, int> BehaviorPriority = new()
    {
        { typeof(TimeoutBehavior), 0 },
        { typeof(NetworkLatencyBehavior), 1 },
        { typeof(StatusCodeBehavior), 2 },
        { typeof(HttpContentBehavior), 3 },
        { typeof(HttpHeaderBehavior), 4 }
    };

    async Task<HttpResponseMessage> IResponseStrategy.ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
    {
        static Task Seed(MockHttpRequestContext requestContext, HttpResponseMessage responseMessage, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        var response = new HttpResponseMessage
        {
            RequestMessage = requestContext.Request
        };

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

    /// <inheritdoc />
    public IWithStatusCodeResult StatusCode(HttpStatusCode statusCode)
    {
        Behaviors.Add(new StatusCodeBehavior(statusCode));
        return this;
    }

    /// <inheritdoc />
    public IWithContentResult Body(Func<MockHttpRequestContext, Task<HttpContent>> httpContentFactory)
    {
        Behaviors.Add(new HttpContentBehavior(httpContentFactory));
        return this;
    }

    /// <inheritdoc />
    public IWithHeadersResult ContentType(MediaTypeHeaderValue mediaType)
    {
        return Headers(new HttpHeadersCollection { { "Content-Type", new[] { mediaType.ToString() } } });
    }

    /// <inheritdoc />
    public IWithHeadersResult Headers(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        // If no content when adding headers, we force empty content so that content headers can still be set.
        if (!Behaviors.OfType<HttpContentBehavior>().Any())
        {
            Body(EmptyHttpContentFactory);
        }

        Behaviors.Add(new HttpHeaderBehavior(headers));
        return this;
    }
}

#nullable restore
