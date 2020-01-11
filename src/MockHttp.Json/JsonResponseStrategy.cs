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
			var serializerSettings = SerializerSettings;
			if (serializerSettings is null)
			{
				requestContext.TryGetService(out serializerSettings);
			}

			object value = ValueFactory(requestContext.Request);
			return Task.FromResult(new HttpResponseMessage(StatusCode)
			{
				Content = new StringContent(JsonConvert.SerializeObject(value, serializerSettings))
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
		public JsonResponseStrategy(HttpStatusCode statusCode, Func<HttpRequestMessage, T> value, MediaTypeHeaderValue mediaType, JsonSerializerSettings serializerSettings)
			: base(statusCode, typeof(T), r => value(r), mediaType, serializerSettings)
		{
		}
	}
}
