using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using HttpClientMock.Http;
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

		public static RequestMatching Headers(this RequestMatching builder, string name, string value)
		{
			return builder.With(new HttpHeadersMatcher(name, value));
		}

		public static RequestMatching Headers<T>(this RequestMatching builder, string name, T value)
			where T : struct
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
			return builder.Headers(name, converter.ConvertToString(value));
		}

		public static RequestMatching Headers(this RequestMatching builder, string headers)
		{
			return builder.Headers(HttpHeadersCollection.Parse(headers));
		}

		public static RequestMatching Headers(this RequestMatching builder, string name, IEnumerable<string> values)
		{
			return builder.With(new HttpHeadersMatcher(name, values));
		}

		public static RequestMatching Headers(this RequestMatching builder, string name, params string[] values)
		{
			return builder.Headers(name, values.AsEnumerable());
		}

		public static RequestMatching Headers(this RequestMatching builder, IEnumerable<KeyValuePair<string, string>> headers)
		{
			return builder.With(new HttpHeadersMatcher(headers));
		}

		public static RequestMatching Headers(this RequestMatching builder, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
		{
			return builder.With(new HttpHeadersMatcher(headers));
		}

		public static RequestMatching ContentType(this RequestMatching builder, string contentType)
		{
			return builder.ContentType(MediaTypeHeaderValue.Parse(contentType));
		}

		public static RequestMatching ContentType(this RequestMatching builder, MediaTypeHeaderValue mediaType)
		{
			if (mediaType == null)
			{
				throw new ArgumentNullException(nameof(mediaType));
			}

			return builder.Headers("Content-Type", mediaType.ToString());
		}

		public static RequestMatching Content(this RequestMatching builder, string content)
		{
			return builder.Replace(new ContentMatcher(content, Encoding.UTF8));
		}

		public static RequestMatching Content(this RequestMatching builder, string content, Encoding encoding)
		{
			return builder.Replace(new ContentMatcher(content, encoding));
		}

		public static RequestMatching Content(this RequestMatching builder, byte[] content)
		{
			return builder.Replace(new ContentMatcher(content));
		}

		public static RequestMatching Content(this RequestMatching builder, Stream stream)
		{
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return builder.Replace(new ContentMatcher(ms.ToArray()));
			}
		}

		public static RequestMatching WithoutContent(this RequestMatching builder)
		{
			return builder.Replace(new ContentMatcher());
		}

		public static RequestMatching PartialContent(this RequestMatching builder, string content, Encoding encoding = null)
		{
			return builder.Replace(new PartialContentMatcher(content, encoding));
		}

		public static RequestMatching PartialContent(this RequestMatching builder, byte[] content)
		{
			return builder.Replace(new PartialContentMatcher(content));
		}

		internal static RequestMatching Any(this RequestMatching builder)
		{
			return builder;
		}

		public static RequestMatching Any(this RequestMatching builder, Action<RequestMatching> anyBuilder)
		{
			var anyRequestMatching = new RequestMatching();
			anyBuilder(anyRequestMatching);
			return builder.With(new AnyMatcher(anyRequestMatching.Build()));
		}
	}
}