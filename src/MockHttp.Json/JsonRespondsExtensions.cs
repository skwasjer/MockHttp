using System.Net;
using System.Net.Http;
#if !NETSTANDARD1_1
using System.Net.Http.Formatting;
#endif
using System.Net.Http.Headers;
using MockHttp.Language;
using MockHttp.Language.Flow;

namespace MockHttp.Json
{
#if !NETSTANDARD1_1
	/// <summary>
	/// JSON and <see cref="MediaTypeFormatter"/> extensions for <see cref="IResponds{TResult}"/>.
	/// </summary>
#else
	/// <summary>
	/// JSON extensions for <see cref="IResponds{TResult}"/>.
	/// </summary>
#endif
	public static class JsonRespondsExtensions
	{
#if !NETSTANDARD1_1
		private static readonly JsonMediaTypeFormatter JsonFormatter = new JsonMediaTypeFormatter();
#endif
		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, T content)
			where TResult : IResponseResult
		{
			return responds.RespondJson(content, (MediaTypeHeaderValue)null);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, T content)
			where TResult : IResponseResult
		{
			return responds.RespondJson(statusCode, content, (MediaTypeHeaderValue)null);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, T content, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			return responds.RespondJson(HttpStatusCode.OK, content, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, T content, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			MediaTypeHeaderValue mt = mediaType ?? MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

#if !NETSTANDARD1_1
			return responds.RespondObject(statusCode, content, JsonFormatter, mt);
#else
			var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(content))
			{
				Headers =
				{
					ContentType = mt
				}
			};
			return responds.Respond(() => new HttpResponseMessage(statusCode)
			{
				Content = jsonContent
			});
#endif
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, T content, string mediaType)
			where TResult : IResponseResult
		{
			return responds.RespondJson(HttpStatusCode.OK, content, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, T content, string mediaType)
			where TResult : IResponseResult
		{
#if !NETSTANDARD1_1
			return responds.RespondObject(statusCode, content, JsonFormatter, mediaType);
#else
			return responds.RespondJson(statusCode, content, mediaType == null ? null : MediaTypeHeaderValue.Parse(mediaType));
#endif
		}

#if !NETSTANDARD1_1
		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, T value, MediaTypeFormatter formatter)
			where TResult : IResponseResult
		{
			return responds.RespondObject(HttpStatusCode.OK, value, formatter);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, T value, MediaTypeFormatter formatter)
			where TResult : IResponseResult
		{
			return responds.RespondObject(statusCode, value, formatter, (MediaTypeHeaderValue)null);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		/// <param name="mediaType">The media type. Can be null, in which case the <paramref name="formatter"/> default content type will be used.</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, T value, MediaTypeFormatter formatter, string mediaType)
			where TResult : IResponseResult
		{
			return responds.RespondObject(HttpStatusCode.OK, value, formatter, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		/// <param name="mediaType">The media type. Can be null, in which case the <paramref name="formatter"/> default content type will be used.</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, T value, MediaTypeFormatter formatter, string mediaType)
			where TResult : IResponseResult
		{
			return responds.RespondObject(statusCode, value, formatter, mediaType == null ? null : MediaTypeHeaderValue.Parse(mediaType));
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		/// <param name="mediaType">The media type. Can be null, in which case the <paramref name="formatter"/> default content type will be used.</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, T value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			return responds.RespondObject(HttpStatusCode.OK, value, formatter, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		/// <param name="mediaType">The media type. Can be null, in which case the <paramref name="formatter"/> default content type will be used.</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, T value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			return responds.Respond(() => new HttpResponseMessage(statusCode)
			{
				Content = new ObjectContent<T>(value, formatter, mediaType)
			});
		}
#endif

	}
}
