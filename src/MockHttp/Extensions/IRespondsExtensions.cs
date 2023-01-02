using MockHttp.Language;
using MockHttp.Language.Flow;
using MockHttp.Language.Flow.Response;
using MockHttp.Responses;

namespace MockHttp;

/// <summary>
/// Extensions for <see cref="IResponds{TResult}" />.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IRespondsExtensions
{
    /// <summary>
    /// Specifies a function that returns the response for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="response">The function that provides the response message to return for a request.</param>
    public static TResult Respond<TResult>(this IResponds<TResult> responds, Func<HttpResponseMessage> response)
        where TResult : IResponseResult
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        return responds.Respond((_, _) => response());
    }

    /// <summary>
    /// Specifies a function that returns the response for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="response">The function that provides the response message to return for given request.</param>
    public static TResult Respond<TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> response)
        where TResult : IResponseResult
    {
        if (responds is null)
        {
            throw new ArgumentNullException(nameof(responds));
        }

        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        return responds.RespondUsing(new ResponseFuncStrategy((request, ct) => Task.FromResult(response(request, ct))));
    }

    /// <summary>
    /// Specifies a function that returns the response for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="with">The function that provides the response message to return for given request.</param>
    public static TResult Respond<TResult>(this IResponds<TResult> responds, Action<MockHttpRequestContext, CancellationToken, IResponseBuilder> with)
        where TResult : IResponseResult
    {
        if (responds is null)
        {
            throw new ArgumentNullException(nameof(responds));
        }

        return responds.RespondUsing(new RequestSpecificResponseBuilder(with));
    }

    /// <summary>
    /// Specifies a strategy that returns the response for a request.
    /// </summary>
    /// <param name="responds"></param>
    internal static TResult RespondUsing<TStrategy, TResult>(this IResponds<TResult> responds)
        where TStrategy : IResponseStrategy, new()
        where TResult : IResponseResult
    {
        if (responds is null)
        {
            throw new ArgumentNullException(nameof(responds));
        }

        return responds.RespondUsing(new TStrategy());
    }

    /// <summary>
    /// Configures a response via the response builder API.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="with">The response builder.</param>
    public static TResult Respond<TResult>(this IResponds<TResult> responds, Action<IResponseBuilder> with)
        where TResult : IResponseResult
    {
        if (responds is null)
        {
            throw new ArgumentNullException(nameof(responds));
        }

        if (with is null)
        {
            throw new ArgumentNullException(nameof(with));
        }

        var builder = new ResponseBuilder();
        with(builder);
        IResponseStrategy responseStrategy = builder.Build();
        return responds.RespondUsing(responseStrategy);
    }

    /// <summary>
    /// Configures a response via the response builder API with access to the request context.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="with">The response builder.</param>
    public static TResult Respond<TResult>(this IResponds<TResult> responds, Action<MockHttpRequestContext, IResponseBuilder> with)
        where TResult : IResponseResult
    {
        if (with is null)
        {
            throw new ArgumentNullException(nameof(with));
        }

        return responds.Respond((ctx, _, builder) => with(ctx, builder));
    }

    private sealed class RequestSpecificResponseBuilder : IResponseStrategy
    {
        private readonly Action<MockHttpRequestContext, CancellationToken, IResponseBuilder> _with;

        public RequestSpecificResponseBuilder(Action<MockHttpRequestContext, CancellationToken, IResponseBuilder> with)
        {
            _with = with ?? throw new ArgumentNullException(nameof(with));
        }

        public Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
        {
            var builder = new ResponseBuilder();
            _with(requestContext, cancellationToken, builder);
            IResponseStrategy responseStrategy = builder.Build();
            return responseStrategy.ProduceResponseAsync(requestContext, cancellationToken);
        }
    }
}
