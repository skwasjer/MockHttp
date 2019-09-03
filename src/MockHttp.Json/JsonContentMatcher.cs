using System.Text;
using System.Threading.Tasks;
using MockHttp.Matchers;
using MockHttp.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MockHttp.Json
{
	internal class JsonContentMatcher : IAsyncHttpRequestMatcher
	{
		private readonly object _jsonContentAsObject;
		private readonly JsonSerializerSettings _serializerSettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="ContentMatcher"/> class using specified raw <paramref name="jsonContentAsObject"/>.
		/// </summary>
		/// <param name="jsonContentAsObject">The request content to match.</param>
		/// <param name="serializerSettings">The JSON serializer settings.</param>
		public JsonContentMatcher(object jsonContentAsObject, JsonSerializerSettings serializerSettings = null)
		{
			_jsonContentAsObject = jsonContentAsObject;
			_serializerSettings = serializerSettings;
		}

		public async Task<bool> IsMatchAsync(MockHttpRequestContext requestContext)
		{
			string requestContent = null;
			if (requestContext.Request.Content != null && requestContext.Request.Content.Headers.ContentLength > 0)
			{
				// Use of ReadAsStringAsync() will use internal buffer, so we can re-enter this method multiple times.
				// In comparison, ReadAsStream() will return the underlying stream which can only be read once.
				requestContent = await requestContext.Request.Content.ReadAsStringAsync().ConfigureAwait(false);
			}

			if (string.IsNullOrEmpty(requestContent) && _jsonContentAsObject == null)
			{
				return true;
			}

			JsonSerializerSettings serializerSettings = _serializerSettings;
			if (serializerSettings == null)
			{
				requestContext.TryGetService(out serializerSettings);
			}

			var requestJsonObject = JsonConvert.DeserializeObject<JObject>(requestContent, serializerSettings);
			var matchingJsonObject = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(_jsonContentAsObject, serializerSettings), serializerSettings);

			return JToken.DeepEquals(requestJsonObject, matchingJsonObject);
		}

		public bool IsExclusive => true;
	}
}