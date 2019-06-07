using System.Net.Http;
using System.Text;
using HttpClientMock.HttpRequestMatchers;

namespace HttpClientMock
{
	public static class RequestMatchingExtensions
	{
		public static RequestMatching Url(this RequestMatching builder, string requestUri)
		{
			return builder.Replace<UrlMatcher>(new UrlMatcher(requestUri));
		}

		public static RequestMatching Method(this RequestMatching builder, string httpMethod)
		{
			return builder.Replace<HttpMethodMatcher>(new HttpMethodMatcher(httpMethod));
		}

		public static RequestMatching Method(this RequestMatching builder, HttpMethod httpMethod)
		{
			return builder.Replace<HttpMethodMatcher>(new HttpMethodMatcher(httpMethod));
		}

		public static RequestMatching Content(this RequestMatching builder, string content, Encoding encoding = null)
		{
			return builder.Replace<ContentMatcher>(new ContentMatcher(content, encoding));
		}

		public static RequestMatching Content(this RequestMatching builder, byte[] content)
		{
			return builder.Replace<ContentMatcher>(new ContentMatcher(content));
		}

		public static RequestMatching WithoutContent(this RequestMatching builder)
		{
			return builder.Replace<ContentMatcher>(new ContentMatcher());
		}
	}
}