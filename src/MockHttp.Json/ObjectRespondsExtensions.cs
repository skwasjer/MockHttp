#if MEDIA_TYPE_FORMATTER
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using MockHttp.Language;
using MockHttp.Language.Flow;

namespace MockHttp.Json
{
	/// <summary>
	/// <see cref="MediaTypeFormatter"/> extensions for <see cref="IResponds{TResult}"/>.
	/// </summary>
	public static class ObjectRespondsExtensions
	{
		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, T value, MediaTypeFormatter formatter)
			where TResult : IResponseResult
		{
			return responds.RespondObject(_ => value, formatter);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, T> value, MediaTypeFormatter formatter)
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
			return responds.RespondObject(statusCode, _ => value, formatter);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Func<HttpRequestMessage, T> value, MediaTypeFormatter formatter)
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
			return responds.RespondObject(_ => value, formatter, mediaType);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		/// <param name="mediaType">The media type. Can be null, in which case the <paramref name="formatter"/> default content type will be used.</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, T> value, MediaTypeFormatter formatter, string mediaType)
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
			return responds.RespondObject(statusCode, _ => value, formatter, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		/// <param name="mediaType">The media type. Can be null, in which case the <paramref name="formatter"/> default content type will be used.</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Func<HttpRequestMessage, T> value, MediaTypeFormatter formatter, string mediaType)
			where TResult : IResponseResult
		{
			return responds.RespondObject(statusCode, value, formatter, mediaType is null ? null : MediaTypeHeaderValue.Parse(mediaType));
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
			return responds.RespondObject(_ => value, formatter, mediaType);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		/// <param name="mediaType">The media type. Can be null, in which case the <paramref name="formatter"/> default content type will be used.</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, T> value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType)
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
			return responds.RespondObject(statusCode, _ => value, formatter, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="value"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="value">The response value.</param>
		/// <param name="formatter">The media type formatter</param>
		/// <param name="mediaType">The media type. Can be null, in which case the <paramref name="formatter"/> default content type will be used.</param>
		public static TResult RespondObject<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Func<HttpRequestMessage, T> value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			return responds.RespondUsing(new MediaTypeFormatterObjectResponseStrategy<T>(statusCode, value, mediaType, formatter));
		}
	}
}
#endif
