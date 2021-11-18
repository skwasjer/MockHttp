using System.Net;
using System.Net.Http.Headers;
using MockHttp.Language;
using MockHttp.Language.Flow;
using Newtonsoft.Json;

namespace MockHttp.Json
{
	/// <summary>
	/// JSON extensions for <see cref="IResponds{TResult}"/>.
	/// </summary>
	public static class JsonRespondsExtensions
	{
		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, T content)
			where TResult : IResponseResult
		{
			return responds.RespondJson(_ => content);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, T> content)
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
			return responds.RespondJson(statusCode, _ => content);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Func<HttpRequestMessage, T> content)
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
			return responds.RespondJson(_ => content, mediaType);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
		/// <param name="serializerSettings">The serializer settings.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, T content, MediaTypeHeaderValue mediaType, JsonSerializerSettings serializerSettings)
			where TResult : IResponseResult
		{
			return responds.RespondJson(_ => content, mediaType, serializerSettings);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, T> content, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			return responds.RespondJson(HttpStatusCode.OK, content, mediaType);
		}


		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
		/// <param name="serializerSettings">The serializer settings.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, T> content, MediaTypeHeaderValue mediaType, JsonSerializerSettings serializerSettings)
			where TResult : IResponseResult
		{
			return responds.RespondJson(HttpStatusCode.OK, content, mediaType, serializerSettings);
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
			return responds.RespondJson(statusCode, _ => content, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Func<HttpRequestMessage, T> content, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			return responds.RespondJson(statusCode, content, mediaType, null);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
		/// <param name="serializerSettings">The serializer settings.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, T content, MediaTypeHeaderValue mediaType, JsonSerializerSettings serializerSettings)
			where TResult : IResponseResult
		{
			return responds.RespondJson(statusCode, _ => content, mediaType, serializerSettings);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
		/// <param name="serializerSettings">The serializer settings.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Func<HttpRequestMessage, T> content, MediaTypeHeaderValue mediaType, JsonSerializerSettings serializerSettings)
			where TResult : IResponseResult
		{
			if (responds is null)
			{
				throw new ArgumentNullException(nameof(responds));
			}

			MediaTypeHeaderValue mt = mediaType ?? MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

			return responds.RespondUsing(new JsonResponseStrategy<T>(statusCode, content, mt, serializerSettings));
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
			return responds.RespondJson(_ => content, mediaType);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, T> content, string mediaType)
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
			return responds.RespondJson(statusCode, _ => content, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type. Can be null, in which case the default JSON content type will be used.</param>
		public static TResult RespondJson<T, TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Func<HttpRequestMessage, T> content, string mediaType)
			where TResult : IResponseResult
		{
			return responds.RespondJson(statusCode, content, mediaType is null ? null : new MediaTypeHeaderValue(mediaType));
		}
	}
}
