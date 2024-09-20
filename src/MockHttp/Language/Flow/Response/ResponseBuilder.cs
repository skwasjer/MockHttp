using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Headers;
using MockHttp.Extensions;
using MockHttp.Http;
using MockHttp.Response.Behaviors;
using MockHttp.Responses;

namespace MockHttp.Language.Flow.Response;

internal sealed class ResponseBuilder
    : IResponseBuilder,
      IWithStatusCodeResult,
      IWithContentResult,
      IWithHeadersResult
{
    /// <inheritdoc />
    public IList<IResponseBehavior> Behaviors { get; } = new List<IResponseBehavior>
    {
        // We're adding this default behavior as the first step, to ensure HttpContent is not null initially.
        new EnsureHttpContentBehavior()
    };

    /// <inheritdoc />
    public IWithStatusCodeResult StatusCode(HttpStatusCode statusCode, string? reasonPhrase = null)
    {
        Behaviors.Replace(new StatusCodeBehavior(statusCode, reasonPhrase));
        return this;
    }

    /// <inheritdoc />
    public IWithContentResult Body(Func<CancellationToken, Task<HttpContent>> httpContentFactory)
    {
        Behaviors.Replace(new HttpContentBehavior(httpContentFactory));
        return this;
    }

    /// <inheritdoc />
    public IWithHeadersResult ContentType(MediaTypeHeaderValue mediaType)
    {
        if (mediaType is null)
        {
            throw new ArgumentNullException(nameof(mediaType));
        }

        return Headers(new HttpHeadersCollection { { HeaderNames.ContentType, mediaType.ToString() } }!);
    }

    /// <inheritdoc />
    public IWithHeadersResult Headers(IEnumerable<KeyValuePair<string, IEnumerable<string?>>> headers)
    {
        if (headers is null)
        {
            throw new ArgumentNullException(nameof(headers));
        }

        var headerList = headers.ToList();
        if (headerList.Count == 0)
        {
            throw new ArgumentException("At least one header must be specified.", nameof(headers));
        }

        Behaviors.Add(new HttpHeaderBehavior(headerList));
        return this;
    }

    internal IResponseStrategy Build()
    {
        return new ResponseStrategy(Behaviors);
    }

    private sealed class ResponseStrategy : IResponseStrategy
    {
        private readonly ReadOnlyCollection<IResponseBehavior> _invertedBehaviors;

        public ResponseStrategy(IEnumerable<IResponseBehavior> behaviors)
        {
            _invertedBehaviors = new ReadOnlyCollection<IResponseBehavior>(
                behaviors
                    .OrderBy(behavior => behavior, new PreferredBehaviorComparer())
                    .Reverse()
                    .ToList()
            );
        }

        public async Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
        {
            static Task Seed(MockHttpRequestContext requestContext, HttpResponseMessage responseMessage, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            var response = new HttpResponseMessage
            {
                RequestMessage = requestContext.Request
            };

            await _invertedBehaviors
                .Aggregate((ResponseHandlerDelegate)Seed,
                    (next, pipeline) => (context, message, ct) => pipeline.HandleAsync(context, message, next, ct)
                )(requestContext, response, cancellationToken)
                .ConfigureAwait(false);

            return response;
        }

        /// <summary>
        /// Some behaviors must be sorted at the top of the list, but may have been added out of order using the Fluent API.
        /// This comparer shifts those preferred behaviors back to the top.
        /// </summary>
        private sealed class PreferredBehaviorComparer : IComparer<IResponseBehavior>
        {
            public int Compare(IResponseBehavior? x, IResponseBehavior? y)
            {
                return Compare(x, y, false);
            }

            private static int Compare(IResponseBehavior? x, IResponseBehavior? y, bool flipped)
            {
                if (ReferenceEquals(x, null))
                {
                    return 1;
                }

                if (ReferenceEquals(y, null))
                {
                    return -1;
                }

                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                return x switch
                {
                    // The network latency behavior must always come first.
                    NetworkLatencyBehavior => -1,
                    // The rate limit behavior must always come first except when the latency behavior is also present.
                    TransferRateBehavior => y is NetworkLatencyBehavior
                        ? 1
                        : CompareOtherWayAround(-1),
                    _ => CompareOtherWayAround(0)
                };

                int CompareOtherWayAround(int result)
                {
                    return flipped
                        ? result
#pragma warning disable S2234 // Parameters to 'Compare' have the same names but not the same order as the method arguments. - justification: intentional.
                        : -Compare(y, x, true);
#pragma warning restore S2234
                }
            }
        }
    }
}
