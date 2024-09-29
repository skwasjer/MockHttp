using System.Net.Http.Headers;
using System.Text;
using MockHttp.Http;
using MockHttp.Json.Extensions;
using MockHttp.Language.Flow.Response;
using MockHttp.Language.Response;
using MockHttp.Responses;

namespace MockHttp.Json;

/// <summary>
/// Response builder extensions.
/// </summary>
public static class ResponseBuilderExtensions
{
    /// <summary>
    /// Sets the JSON content for the response.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="jsonContent">The object to be returned as JSON.</param>
    /// <param name="encoding">The optional JSON encoding.</param>
    /// <param name="adapter">The optional JSON adapter. When null uses the default adapter.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is <see langword="null" />.</exception>
    public static IWithContentResult JsonBody<T>(this IWithContent builder, T jsonContent, Encoding? encoding = null, IJsonAdapter? adapter = null)
    {
        return builder.JsonBody(() => jsonContent, encoding, adapter);
    }

    /// <summary>
    /// Sets the JSON content for the response using a factory returning a new instance of <typeparamref name="T"/> on each invocation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="jsonContentFactory">The factory which creates an object to be returned as JSON.</param>
    /// <param name="encoding">The optional JSON encoding.</param>
    /// <param name="adapter">The optional JSON adapter. When null uses the default adapter.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="jsonContentFactory" /> is <see langword="null" />.</exception>
    public static IWithContentResult JsonBody<T>
    (
        this IWithContent builder,
        Func<T> jsonContentFactory,
        Encoding? encoding = null,
        IJsonAdapter? adapter = null
    )
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Behaviors.Add(new JsonContentBehavior<T>(jsonContentFactory, encoding, adapter));
        return (IWithContentResult)builder;
    }

    private sealed class JsonContentBehavior<T> : IResponseBehavior
    {
        private readonly Func<T> _jsonContentFactory;
        private readonly Encoding? _encoding;
        private readonly IJsonAdapter? _adapter;

        internal JsonContentBehavior
        (
            Func<T> jsonContentFactory,
            Encoding? encoding = null,
            IJsonAdapter? adapter = null
        )
        {
            _jsonContentFactory = jsonContentFactory ?? throw new ArgumentNullException(nameof(jsonContentFactory));
            _encoding = encoding;
            _adapter = adapter;
        }

        public Task HandleAsync(
            MockHttpRequestContext requestContext,
            HttpResponseMessage responseMessage,
            ResponseHandler nextHandler,
            CancellationToken cancellationToken
        )
        {
            IJsonAdapter jsonSerializerAdapter = _adapter ?? requestContext.GetAdapter();
            object? value = _jsonContentFactory();

            responseMessage.Content = new StringContent(jsonSerializerAdapter.Serialize(value), _encoding)
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue(MediaTypes.Json) { CharSet = (_encoding ?? MockHttpHandler.DefaultEncoding).WebName }
                }
            };

            return nextHandler(requestContext, responseMessage, cancellationToken);
        }
    }
}
