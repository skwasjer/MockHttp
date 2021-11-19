using System.Net;
using System.Net.Http.Headers;
using MockHttp.Json.Extensions;
using MockHttp.Responses;

namespace MockHttp.Json;

internal class JsonResponseStrategy : ObjectResponseStrategy
{
    private readonly IJsonAdapter? _adapter;

    public JsonResponseStrategy
    (
        HttpStatusCode statusCode,
        Type typeOfValue,
        Func<HttpRequestMessage, object?> valueFactory,
        MediaTypeHeaderValue mediaType,
        IJsonAdapter? adapter)
        : base(statusCode, typeOfValue, valueFactory, mediaType)
    {
        _adapter = adapter;
    }

    public override Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
    {
        IJsonAdapter jsonSerializerAdapter = _adapter ?? requestContext.GetAdapter();
        object? value = ValueFactory(requestContext.Request);
        return Task.FromResult(new HttpResponseMessage(StatusCode)
        {
            Content = new StringContent(jsonSerializerAdapter.Serialize(value))
            {
                Headers =
                {
                    ContentType = MediaType ?? MediaTypeHeaderValue.Parse(MediaTypes.JsonMediaTypeWithUtf8)
                }
            }
        });
    }
}

internal class JsonResponseStrategy<T> : JsonResponseStrategy
{
    public JsonResponseStrategy(HttpStatusCode statusCode, Func<HttpRequestMessage, T> valueFactory, MediaTypeHeaderValue mediaType, IJsonAdapter? adapter)
        : base(
            statusCode,
            typeof(T),
            r => valueFactory is not null
                ? valueFactory(r)
                : throw new ArgumentNullException(nameof(valueFactory)),
            mediaType,
            adapter
        )
    {
    }
}
