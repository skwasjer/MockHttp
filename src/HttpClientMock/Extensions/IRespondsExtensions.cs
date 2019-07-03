using System;
using System.IO;
using System.Net;
using System.Net.Http;
#if !NETSTANDARD1_1
using System.Net.Http.Formatting;
#endif
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HttpClientMock.Language;
using HttpClientMock.Language.Flow;

namespace HttpClientMock
{
	/// <summary>
	/// Extensions for <see cref="IResponds"/>.
	/// </summary>
	public static class IRespondsExtensions
	{
		/// <summary>
		/// Specifies a function that returns the response for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="response">The function that provides the response message to return for a request.</param>
		public static IResponseResult RespondWith(this IResponds responds, Func<HttpResponseMessage> response)
		{
			return responds.RespondWith(_ => response());
		}

		/// <summary>
		/// Specifies a function that returns the response for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="response">The function that provides the response message to return for given request.</param>
		public static IResponseResult RespondWith(this IResponds responds, Func<HttpRequestMessage, HttpResponseMessage> response)
		{
			return responds.RespondWith(request => Task.FromResult(response(request)));
		}

		/// <summary>
		/// Specifies the <paramref name="response"/> message for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="response">The response message to return for given request.</param>
		public static IResponseResult RespondWith(this IResponds responds, HttpResponseMessage response)
		{
			return responds.RespondWith(() => response);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> response for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		public static IResponseResult RespondWith(this IResponds responds, HttpStatusCode statusCode)
		{
			return responds.RespondWith(new HttpResponseMessage(statusCode));
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/>, <paramref name="content"/> and <paramref name="mediaType"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type.</param>
		public static IResponseResult RespondWith(this IResponds responds, HttpStatusCode statusCode, string content, string mediaType)
		{
			var sc = new StringContent(content);
			sc.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
			return responds.RespondWith(new HttpResponseMessage(statusCode)
			{
				Content = sc
			});
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/>, <paramref name="content"/>, <paramref name="encoding"/> and <paramref name="mediaType"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="encoding">The encoding.</param>
		/// <param name="mediaType">The media type.</param>
		public static IResponseResult RespondWith(this IResponds responds, HttpStatusCode statusCode, string content, Encoding encoding, string mediaType)
		{
			return responds.RespondWith(new HttpResponseMessage(statusCode)
			{
				Content = new StringContent(content, encoding, mediaType)
			});
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		public static IResponseResult RespondWith(this IResponds responds, HttpStatusCode statusCode, Stream content)
		{
			return responds.RespondWith(new HttpResponseMessage(statusCode)
			{
				Content = new StreamContent(content)
			});
		}


#if !NETSTANDARD1_1
		private static readonly JsonMediaTypeFormatter JsonFormatter = new JsonMediaTypeFormatter();

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		public static IResponseResult RespondWith<T>(this IResponds responds, HttpStatusCode statusCode, T content)
		{
			return responds.RespondWith(statusCode, content, JsonFormatter);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaTypeFormatter">The media type formatter</param>
		public static IResponseResult RespondWith<T>(this IResponds responds, HttpStatusCode statusCode, T content, MediaTypeFormatter mediaTypeFormatter)
		{
			return responds.RespondWith(new HttpResponseMessage(statusCode)
			{
				Content = new ObjectContent<T>(content, mediaTypeFormatter)
			});
		}
#endif
	}
}
