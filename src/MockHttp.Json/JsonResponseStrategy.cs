using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using MockHttp.Responses;
using Newtonsoft.Json;

namespace MockHttp.Json
{
	internal class JsonResponseStrategy : ObjectResponseStrategy
	{
		public JsonResponseStrategy(HttpStatusCode statusCode, Type typeOfValue, Func<HttpRequestMessage, object> valueFactory, MediaTypeHeaderValue mediaType, JsonSerializerSettings serializerSettings)
			: base(statusCode, typeOfValue, valueFactory, mediaType)
		{
			SerializerSettings = serializerSettings;
		}

		protected JsonSerializerSettings SerializerSettings { get; }

		public override Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
		{
			requestContext.TryGetService(out JsonSerializerSettings serializerSettings);
			return Task.FromResult(new HttpResponseMessage(StatusCode)
			{
				Content = new StringContent(JsonConvert.SerializeObject(ValueFactory(requestContext.Request), serializerSettings ?? SerializerSettings))
				{
					Headers =
					{
						ContentType = MediaType ?? MediaTypeHeaderValue.Parse(MediaTypes.JsonMediaType)
					}
				}
			});
		}
	}

	internal class JsonResponseStrategy<T> : JsonResponseStrategy
	{
		public JsonResponseStrategy(HttpStatusCode statusCode, Func<HttpRequestMessage, T> value, MediaTypeHeaderValue mediaType, JsonSerializerSettings serializerSettings)
			: base(statusCode, typeof(T), r => value(r), mediaType, serializerSettings)
		{
		}
	}
}
