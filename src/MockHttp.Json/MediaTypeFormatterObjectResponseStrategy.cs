#if MEDIA_TYPE_FORMATTER
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MockHttp.Responses;

namespace MockHttp.Json
{
	internal class MediaTypeFormatterObjectResponseStrategy : ObjectResponseStrategy
	{
		public MediaTypeFormatterObjectResponseStrategy(HttpStatusCode statusCode, Type typeOfValue, Func<HttpRequestMessage, object> valueFactory, MediaTypeHeaderValue mediaType, MediaTypeFormatter formatter)
			: base(statusCode, typeOfValue, valueFactory, mediaType)
		{
			Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
		}

		protected MediaTypeFormatter Formatter { get; }

		public override Task<HttpResponseMessage> ProduceResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return Task.FromResult(new HttpResponseMessage(StatusCode)
			{
				Content = new ObjectContent(TypeOfValue, ValueFactory(request), Formatter, MediaType)
			});
		}
	}

	internal class MediaTypeFormatterObjectResponseStrategy<T> : MediaTypeFormatterObjectResponseStrategy
	{
		public MediaTypeFormatterObjectResponseStrategy(HttpStatusCode statusCode, Func<HttpRequestMessage, T> value, MediaTypeHeaderValue mediaType, MediaTypeFormatter formatter)
			: base(statusCode, typeof(T), r => value(r), mediaType, formatter)
		{
		}

		public override Task<HttpResponseMessage> ProduceResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			MediaTypeFormatter formatter = Formatter.GetPerRequestFormatterInstance(TypeOfValue, request, MediaType);
			return Task.FromResult(new HttpResponseMessage(StatusCode)
			{
				Content = new ObjectContent<T>((T)ValueFactory(request), formatter, MediaType)
			});
		}
	}
}
#endif