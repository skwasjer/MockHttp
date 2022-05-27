using System.Net;
using System.Net.Http.Headers;
using System.Text;
using MockHttp.Language;
using MockHttp.Language.Flow;
using MockHttp.Language.Flow.Response;
using MockHttp.Language.Response;

namespace MockHttp.Json;

/// <summary>
/// JSON extensions for <see cref="IResponds{TResult}" />.
/// </summary>
[Obsolete(DeprecationWarnings.RespondsExtensions, false)]
public static class RespondsExtensions
{
    /// <summary>
    /// Specifies the <see cref="HttpStatusCode.OK" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="content">The response content.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, T content)
        where TResult : IResponseResult
    {
        return responds.RespondJson(_ => content);
    }

    /// <summary>
    /// Specifies the <see cref="HttpStatusCode.OK" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="content">The response content.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, T> content)
        where TResult : IResponseResult
    {
        return responds.RespondJson(content, (MediaTypeHeaderValue?)null);
    }

    /// <summary>
    /// Specifies the <paramref name="statusCode" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="statusCode">The status code response for given request.</param>
    /// <param name="content">The response content.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, T content)
        where TResult : IResponseResult
    {
        return responds.RespondJson(statusCode, _ => content);
    }

    /// <summary>
    /// Specifies the <paramref name="statusCode" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="statusCode">The status code response for given request.</param>
    /// <param name="content">The response content.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Func<HttpRequestMessage, T> content)
        where TResult : IResponseResult
    {
        return responds.RespondJson(statusCode, content, (MediaTypeHeaderValue?)null);
    }

    /// <summary>
    /// Specifies the <see cref="HttpStatusCode.OK" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="content">The response content.</param>
    /// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, T content, MediaTypeHeaderValue? mediaType)
        where TResult : IResponseResult
    {
        return responds.RespondJson(_ => content, mediaType);
    }

    /// <summary>
    /// Specifies the <see cref="HttpStatusCode.OK" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="content">The response content.</param>
    /// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, T> content, MediaTypeHeaderValue? mediaType)
        where TResult : IResponseResult
    {
        return responds.RespondJson(HttpStatusCode.OK, content, mediaType);
    }

    /// <summary>
    /// Specifies the <paramref name="statusCode" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="statusCode">The status code response for given request.</param>
    /// <param name="content">The response content.</param>
    /// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, T content, MediaTypeHeaderValue? mediaType)
        where TResult : IResponseResult
    {
        return responds.RespondJson(statusCode, _ => content, mediaType);
    }

    /// <summary>
    /// Specifies the <paramref name="statusCode" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="statusCode">The status code response for given request.</param>
    /// <param name="content">The response content.</param>
    /// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Func<HttpRequestMessage, T> content, MediaTypeHeaderValue? mediaType)
        where TResult : IResponseResult
    {
        return responds.RespondJson(statusCode, content, mediaType, null);
    }

    /// <summary>
    /// Specifies the <paramref name="statusCode" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="statusCode">The status code response for given request.</param>
    /// <param name="content">The response content.</param>
    /// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
    /// <param name="adapter">The JSON adapter.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Func<HttpRequestMessage, T> content, MediaTypeHeaderValue? mediaType, IJsonAdapter? adapter)
        where TResult : IResponseResult
    {
        if (responds is null)
        {
            throw new ArgumentNullException(nameof(responds));
        }

        Encoding? enc = mediaType?.CharSet is not null
            ? Encoding.GetEncoding(mediaType.CharSet)
            : null;

        IWithResponse With(HttpRequestMessage request, IWithStatusCode with)
        {
            IWithContentResult builder = with
                .StatusCode(statusCode)
                .JsonBody(() => content(request), enc, adapter);

            if (mediaType is not null)
            {
                return builder.ContentType(mediaType);
            }

            return builder;
        }

        return responds.Respond((ctx, with) => With(ctx.Request, with));
    }

    /// <summary>
    /// Specifies the <see cref="HttpStatusCode.OK" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="content">The response content.</param>
    /// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, T content, string? mediaType)
        where TResult : IResponseResult
    {
        return responds.RespondJson(_ => content, mediaType);
    }

    /// <summary>
    /// Specifies the <see cref="HttpStatusCode.OK" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="content">The response content.</param>
    /// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, T> content, string? mediaType)
        where TResult : IResponseResult
    {
        return responds.RespondJson(HttpStatusCode.OK, content, mediaType);
    }

    /// <summary>
    /// Specifies the <paramref name="statusCode" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="statusCode">The status code response for given request.</param>
    /// <param name="content">The response content.</param>
    /// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, T content, string? mediaType)
        where TResult : IResponseResult
    {
        return responds.RespondJson(statusCode, _ => content, mediaType);
    }

    /// <summary>
    /// Specifies the <paramref name="statusCode" /> and <paramref name="content" /> to respond with for a request.
    /// </summary>
    /// <param name="responds"></param>
    /// <param name="statusCode">The status code response for given request.</param>
    /// <param name="content">The response content.</param>
    /// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
    [Obsolete(DeprecationWarnings.RespondsExtensions, false)]
    public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Func<HttpRequestMessage, T> content, string? mediaType)
        where TResult : IResponseResult
    {
        return responds.RespondJson(statusCode, content, mediaType is null ? null : new MediaTypeHeaderValue(mediaType));
    }
}
