using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using MockHttp.Responses;

namespace MockHttp.Json;

internal class MediaTypeFormatterObjectResponseStrategy : ObjectResponseStrategy
{
	public MediaTypeFormatterObjectResponseStrategy(HttpStatusCode statusCode, Type typeOfValue, Func<HttpRequestMessage, object> valueFactory, MediaTypeHeaderValue mediaType, MediaTypeFormatter formatter)
		: base(statusCode, typeOfValue, valueFactory, mediaType)
	{
		Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
	}

	protected MediaTypeFormatter Formatter { get; }

	public override Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
	{
		return Task.FromResult(new HttpResponseMessage(StatusCode)
		{
			Content = new ObjectContent(TypeOfValue, ValueFactory(requestContext.Request), Formatter, MediaType)
		});
	}
}

internal class MediaTypeFormatterObjectResponseStrategy<T> : MediaTypeFormatterObjectResponseStrategy
{
	public MediaTypeFormatterObjectResponseStrategy(HttpStatusCode statusCode, Func<HttpRequestMessage, T> valueFactory, MediaTypeHeaderValue mediaType, MediaTypeFormatter formatter)
		: base(
			statusCode,
			typeof(T),
			r => valueFactory != null ? valueFactory(r) : throw new ArgumentNullException(nameof(valueFactory)),
			mediaType,
			formatter)
	{
	}

	public override Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
	{
		MediaTypeFormatter formatter = Formatter.GetPerRequestFormatterInstance(TypeOfValue, requestContext.Request, MediaType);
		return Task.FromResult(new HttpResponseMessage(StatusCode)
		{
			Content = new ObjectContent<T>((T)ValueFactory(requestContext.Request), formatter, MediaType)
		});
	}
}