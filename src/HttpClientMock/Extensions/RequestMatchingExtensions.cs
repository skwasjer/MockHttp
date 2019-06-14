using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using HttpClientMock.Matchers;

namespace HttpClientMock
{
	public static class RequestMatchingExtensions
	{
		public static RequestMatching Url(this RequestMatching builder, string requestUri)
		{
			return builder.With(new UrlMatcher(requestUri));
		}
		public static RequestMatching QueryString(this RequestMatching builder, string key, string value)
		{
			return builder.QueryString(key, new[] { value });
		}

		public static RequestMatching QueryString(this RequestMatching builder, string key, IEnumerable<string> values)
		{
			return builder.QueryString(new Dictionary<string, IEnumerable<string>>
			{
				{ key, values }
			});
		}

		public static RequestMatching QueryString(this RequestMatching builder, string key, params string[] values)
		{
			return builder.QueryString(key, values?.AsEnumerable());
		}

		public static RequestMatching QueryString(this RequestMatching builder, IEnumerable<KeyValuePair<string, IEnumerable<string>>> values)
		{
			return builder.With(new QueryStringMatcher(values));
		}

		public static RequestMatching QueryString(this RequestMatching builder, string queryString)
		{
			return builder.With(new QueryStringMatcher(queryString));
		}

		public static RequestMatching Method(this RequestMatching builder, string httpMethod)
		{
			return builder.With(new HttpMethodMatcher(httpMethod));
		}

		public static RequestMatching Method(this RequestMatching builder, HttpMethod httpMethod)
		{
			return builder.With(new HttpMethodMatcher(httpMethod));
		}

		public static RequestMatching Content(this RequestMatching builder, string content, Encoding encoding = null)
		{
			return builder.Replace(new ContentMatcher(content, encoding));
		}

		public static RequestMatching Content(this RequestMatching builder, byte[] content)
		{
			return builder.Replace(new ContentMatcher(content));
		}

		public static RequestMatching WithoutContent(this RequestMatching builder)
		{
			return builder.Replace(new ContentMatcher());
		}
	}
}