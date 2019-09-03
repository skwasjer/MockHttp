using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using MockHttp.Http;
using MockHttp.Matchers;

namespace MockHttp
{
	/// <summary>
	/// Extensions for <see cref="RequestMatching"/>.
	/// </summary>
	public static class RequestMatchingExtensions
	{
		/// <summary>
		/// Matches a request by specified <paramref name="requestUri"/>.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="requestUri">The request URI or a URI wildcard.</param>
		/// <returns>The request matching builder instance.</returns>
		[Obsolete("Use `RequestUri` instead, will be removed.")]
		public static RequestMatching Url(this RequestMatching builder, string requestUri)
		{
			return builder.RequestUri(requestUri);
		}

		/// <summary>
		/// Matches a request by specified <paramref name="requestUri"/>.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="requestUri">The request URI or a URI wildcard.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching RequestUri(this RequestMatching builder, string requestUri)
		{
			return builder.With(new RequestUriMatcher(requestUri, true));
		}

		/// <summary>
		/// Matches a request by specified <paramref name="requestUri"/>.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="requestUri">The request URI or a URI wildcard.</param>
		/// <returns>The request matching builder instance.</returns>
		[Obsolete("Use `RequestUri` instead, will be removed.")]
		public static RequestMatching Url(this RequestMatching builder, Uri requestUri)
		{
			return builder.RequestUri(requestUri);
		}

		/// <summary>
		/// Matches a request by specified <paramref name="requestUri"/>.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="requestUri">The relative or absolute request URI.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching RequestUri(this RequestMatching builder, Uri requestUri)
		{
			return builder.With(new RequestUriMatcher(requestUri));
		}

		/// <summary>
		/// Matches a request by query string.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="key">The query string parameter key.</param>
		/// <param name="value">The query string value.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching QueryString(this RequestMatching builder, string key, string value)
		{
			return builder.QueryString(
				key,
				value == null
#if NETSTANDARD2_0
					? Array.Empty<string>()
#else
					? new string[0]
#endif
					: new[] { value });
		}

		/// <summary>
		/// Matches a request by query string.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="key">The query string parameter key.</param>
		/// <param name="values">The query string values.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching QueryString(this RequestMatching builder, string key, IEnumerable<string> values)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			return builder.QueryString(new Dictionary<string, IEnumerable<string>>
			{
				{ key, values }
			});
		}

		/// <summary>
		/// Matches a request by query string.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="key">The query string parameter key.</param>
		/// <param name="values">The query string value.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching QueryString(this RequestMatching builder, string key, params string[] values)
		{
			return builder.QueryString(key, values?.AsEnumerable());
		}

		/// <summary>
		/// Matches a request by query string.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="parameters">The query string parameters.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching QueryString(this RequestMatching builder, IEnumerable<KeyValuePair<string, IEnumerable<string>>> parameters)
		{
			return builder.With(new QueryStringMatcher(parameters));
		}

#if !NETSTANDARD1_1
		/// <summary>
		/// Matches a request by query string.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="parameters">The query string parameters.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching QueryString(this RequestMatching builder, NameValueCollection parameters)
		{
			return builder.With(new QueryStringMatcher(parameters?.AsEnumerable()));
		}
#endif

		/// <summary>
		/// Matches a request by query string.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="queryString">The query string.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching QueryString(this RequestMatching builder, string queryString)
		{
			if (string.IsNullOrEmpty(queryString))
			{
				throw new ArgumentException("Specify a query string, or use 'WithoutQueryString'.", nameof(queryString));
			}

			return builder.With(new QueryStringMatcher(queryString));
		}

		/// <summary>
		/// Matches a request explicitly that has no request content.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching WithoutQueryString(this RequestMatching builder)
		{
			return builder.With(new QueryStringMatcher(""));
		}

		/// <summary>
		/// Matches a request by HTTP method.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Method(this RequestMatching builder, string httpMethod)
		{
			return builder.Method(new HttpMethod(httpMethod));
		}

		/// <summary>
		/// Matches a request by HTTP method.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Method(this RequestMatching builder, HttpMethod httpMethod)
		{
			return builder.With(new HttpMethodMatcher(httpMethod));
		}

		/// <summary>
		/// Matches a request by HTTP header.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="name">The header name.</param>
		/// <param name="value">The header value.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Header(this RequestMatching builder, string name, string value)
		{
			return builder.With(new HttpHeadersMatcher(name, value));
		}

		/// <summary>
		/// Matches a request by HTTP header. A type converter is used to convert the <paramref name="value"/> to string.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="name">The header name.</param>
		/// <param name="value">The header value.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Header<T>(this RequestMatching builder, string name, T value)
			where T : struct
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
			return builder.Header(name, converter.ConvertToString(value));
		}

		/// <summary>
		/// Matches a request by HTTP header on a datetime value (per RFC-2616).
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="name">The header name.</param>
		/// <param name="date">The header value.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Header(this RequestMatching builder, string name, DateTime date)
		{
			return builder.Header(name, (DateTimeOffset)date.ToUniversalTime());
		}

		/// <summary>
		/// Matches a request by HTTP header on a datetime value (per RFC-2616).
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="name">The header name.</param>
		/// <param name="date">The header value.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Header(this RequestMatching builder, string name, DateTimeOffset date)
		{
			// https://tools.ietf.org/html/rfc2616#section-3.3.1
			CultureInfo ci = CultureInfo.InvariantCulture;
#if NETFRAMEWORK
			// .NET Framework does not normalize other common date formats,
			// so we use multiple matches.
			return builder.Any(any => any
				.Header(name, date.ToString("R", ci))
				.Header(name, date.ToString("dddd, dd-MMM-yy HH:mm:ss 'GMT'", ci))	// RFC 1036
				.Header(name, date.ToString("ddd MMM  d  H:mm:ss yyyy", ci))		// ANSI C's asctime()
			);
#else
			return builder.Header(name, date.ToString("R", ci));
#endif
		}

		/// <summary>
		/// Matches a request by HTTP header.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="name">The header name.</param>
		/// <param name="values">The header values.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Header(this RequestMatching builder, string name, IEnumerable<string> values)
		{
			return builder.With(new HttpHeadersMatcher(name, values));
		}

		/// <summary>
		/// Matches a request by HTTP headers.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="headers">The headers.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Headers(this RequestMatching builder, string headers)
		{
			return builder.Headers(HttpHeadersCollection.Parse(headers));
		}

		/// <summary>
		/// Matches a request by HTTP header.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="name">The header name.</param>
		/// <param name="values">The header values.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Headers(this RequestMatching builder, string name, params string[] values)
		{
			return builder.Header(name, values.AsEnumerable());
		}

		/// <summary>
		/// Matches a request by HTTP header.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="headers">The headers.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Headers(this RequestMatching builder, IEnumerable<KeyValuePair<string, string>> headers)
		{
			return builder.With(new HttpHeadersMatcher(headers));
		}

		/// <summary>
		/// Matches a request by HTTP header.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="headers">The headers.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Headers(this RequestMatching builder, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
		{
			return builder.With(new HttpHeadersMatcher(headers));
		}

		/// <summary>
		/// Matches a request by content type.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="mediaType">The content type.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching ContentType(this RequestMatching builder, string mediaType)
		{
			if (mediaType == null)
			{
				throw new ArgumentNullException(nameof(mediaType));
			}

			return builder.ContentType(MediaTypeHeaderValue.Parse(mediaType));
		}

		/// <summary>
		/// Matches a request by content type.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="contentType">The content type.</param>
		/// <param name="encoding">The content encoding.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching ContentType(this RequestMatching builder, string contentType, Encoding encoding)
		{
			if (contentType == null)
			{
				throw new ArgumentNullException(nameof(contentType));
			}

			var mediaType = new MediaTypeHeaderValue(contentType)
			{
				CharSet = encoding?.WebName
			};
			return builder.ContentType(mediaType);
		}

		/// <summary>
		/// Matches a request by media type.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="mediaType">The media type.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching ContentType(this RequestMatching builder, MediaTypeHeaderValue mediaType)
		{
			if (mediaType == null)
			{
				throw new ArgumentNullException(nameof(mediaType));
			}

			return builder.With(new MediaTypeHeaderMatcher(mediaType));
		}

		/// <summary>
		/// Matches a request by form data.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="key">The form data parameter key.</param>
		/// <param name="value">The form data value.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching FormData(this RequestMatching builder, string key, string value)
		{
			return builder.FormData(new [] { new KeyValuePair<string, string>(key, value) });
		}

		/// <summary>
		/// Matches a request by form data.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="formData">The form data parameters.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching FormData(this RequestMatching builder, IEnumerable<KeyValuePair<string, string>> formData)
		{
			return builder.With(new FormDataMatcher(formData));
		}

		/// <summary>
		/// Matches a request by form data.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="urlEncodedFormData">The form data.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching FormData(this RequestMatching builder, string urlEncodedFormData)
		{
			if (string.IsNullOrEmpty(urlEncodedFormData))
			{
				throw new ArgumentException("Specify the url encoded form data.", nameof(urlEncodedFormData));
			}

			return builder.With(new FormDataMatcher(urlEncodedFormData));
		}

		/// <summary>
		/// Matches a request by request content.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="content">The request content.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Content(this RequestMatching builder, string content)
		{
			return builder.Content(content, ContentMatcher.DefaultEncoding);
		}

		/// <summary>
		/// Matches a request by request content.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="content">The request content.</param>
		/// <param name="encoding">The request content encoding.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Content(this RequestMatching builder, string content, Encoding encoding)
		{
			return builder.With(new ContentMatcher(content, encoding));
		}

		/// <summary>
		/// Matches a request by request content.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="content">The request content.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Content(this RequestMatching builder, byte[] content)
		{
			return builder.With(new ContentMatcher(content));
		}

		/// <summary>
		/// Matches a request by request content.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="content">The request content.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Content(this RequestMatching builder, Stream content)
		{
			using (var ms = new MemoryStream())
			{
				content.CopyTo(ms);
				return builder.Content(ms.ToArray());
			}
		}

		/// <summary>
		/// Matches a request explicitly that has no request content.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching WithoutContent(this RequestMatching builder)
		{
			return builder.With(new ContentMatcher());
		}

		/// <summary>
		/// Matches a request by partially matching the request content.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="partialContent">The request content.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching PartialContent(this RequestMatching builder, string partialContent)
		{
			return builder.PartialContent(partialContent, ContentMatcher.DefaultEncoding);
		}

		/// <summary>
		/// Matches a request by partially matching the request content.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="partialContent">The request content.</param>
		/// <param name="encoding">The request content encoding.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching PartialContent(this RequestMatching builder, string partialContent, Encoding encoding)
		{
			return builder.With(new PartialContentMatcher(partialContent, encoding));
		}

		/// <summary>
		/// Matches a request by partially matching the request content.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="partialContent">The request content.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching PartialContent(this RequestMatching builder, byte[] partialContent)
		{
			return builder.With(new PartialContentMatcher(partialContent));
		}

		/// <summary>
		/// Matches a request by partially matching the request content.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="content">The request content.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching PartialContent(this RequestMatching builder, Stream content)
		{
			using (var ms = new MemoryStream())
			{
				content.CopyTo(ms);
				return builder.PartialContent(ms.ToArray());
			}
		}

		/// <summary>
		/// Matches a request by verifying it against a list of constraints, for which at least one has to match the request.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="anyBuilder">An action to configure an inner request matching builder.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Any(this RequestMatching builder, Action<RequestMatching> anyBuilder)
		{
			var anyRequestMatching = new AnyRequestMatching();
			anyBuilder(anyRequestMatching);
			return builder.With(new AnyMatcher(anyRequestMatching.Build()));
		}

		/// <summary>
		/// Matches a request using a custom expression.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>The request matching builder instance.</returns>
		[Obsolete("Replaced by " + nameof(Where) + ".")]
		public static RequestMatching When(this RequestMatching builder, Expression<Func<HttpRequestMessage, bool>> expression)
		{
			return builder.Where(expression);
		}

		/// <summary>
		/// Matches a request using a custom expression.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Where(this RequestMatching builder, Expression<Func<HttpRequestMessage, bool>> expression)
		{
			return builder.With(new ExpressionMatcher(expression));
		}

		/// <summary>
		/// Matches a request by matching the request message version.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="version">The message version.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Version(this RequestMatching builder, string version)
		{
			return builder.Version(version == null ? null : System.Version.Parse(version));
		}

		/// <summary>
		/// Matches a request by matching the request message version.
		/// </summary>
		/// <param name="builder">The request matching builder instance.</param>
		/// <param name="version">The message version.</param>
		/// <returns>The request matching builder instance.</returns>
		public static RequestMatching Version(this RequestMatching builder, Version version)
		{
			return builder.With(new VersionMatcher(version));
		}
	}
}