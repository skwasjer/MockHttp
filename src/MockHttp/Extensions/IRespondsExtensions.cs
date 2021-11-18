using System.Net;
using System.Net.Http.Headers;
using System.Text;
using MockHttp.Language;
using MockHttp.Language.Flow;
using MockHttp.Responses;

namespace MockHttp
{
	/// <summary>
	/// Extensions for <see cref="IResponds{TResult}"/>.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static class IRespondsExtensions
	{
		/// <summary>
		/// Specifies a function that returns the response for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="response">The function that provides the response message to return for a request.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, Func<HttpResponseMessage> response)
			where TResult : IResponseResult
		{
			if (responds is null)
			{
				throw new ArgumentNullException(nameof(responds));
			}

			if (response is null)
			{
				throw new ArgumentNullException(nameof(response));
			}

			return responds.Respond((_, _) => Task.FromResult(response()));
		}

		/// <summary>
		/// Specifies a function that returns the response for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="response">The function that provides the response message to return for given request.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, HttpResponseMessage> response)
			where TResult : IResponseResult
		{
			if (responds is null)
			{
				throw new ArgumentNullException(nameof(responds));
			}

			if (response is null)
			{
				throw new ArgumentNullException(nameof(response));
			}

			return responds.Respond((request, _) => Task.FromResult(response(request)));
		}

		/// <summary>
		/// Specifies a strategy that returns the response for a request.
		/// </summary>
		/// <param name="responds"></param>
		internal static TResult RespondUsing<TStrategy, TResult>(this IResponds<TResult> responds)
			where TStrategy : IResponseStrategy, new()
			where TResult : IResponseResult
		{
			if (responds is null)
			{
				throw new ArgumentNullException(nameof(responds));
			}

			return responds.RespondUsing(new TStrategy());
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> response for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode)
			where TResult : IResponseResult
		{
			return responds.Respond(() => new HttpResponseMessage(statusCode));
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, string content)
			where TResult : IResponseResult
		{
			return responds.Respond(HttpStatusCode.OK, content);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, string content)
			where TResult : IResponseResult
		{
			return responds.Respond(statusCode, content, (string)null);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/>, <paramref name="content"/> and <paramref name="mediaType"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, string content, string mediaType)
			where TResult : IResponseResult
		{
			return responds.Respond(HttpStatusCode.OK, content, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/>, <paramref name="content"/> and <paramref name="mediaType"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, string content, string mediaType)
			where TResult : IResponseResult
		{
			return responds.Respond(statusCode, content, null, mediaType);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/>, <paramref name="content"/> and <paramref name="mediaType"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, string content, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			return responds.Respond(HttpStatusCode.OK, content, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/>, <paramref name="content"/> and <paramref name="mediaType"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, string content, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			if (content is null)
			{
				throw new ArgumentNullException(nameof(content));
			}

			return responds.Respond(() => new HttpResponseMessage(statusCode)
			{
				Content = new StringContent(content)
				{
					Headers =
					{
						ContentType = mediaType
					}
				}
			});
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/>, <paramref name="content"/>, <paramref name="encoding"/> and <paramref name="mediaType"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		/// <param name="encoding">The encoding.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, string content, Encoding encoding, string mediaType)
			where TResult : IResponseResult
		{
			return responds.Respond(HttpStatusCode.OK, content, encoding, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/>, <paramref name="content"/>, <paramref name="encoding"/> and <paramref name="mediaType"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="encoding">The encoding.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, string content, Encoding encoding, string mediaType)
			where TResult : IResponseResult
		{
			return responds.Respond(statusCode, content, new MediaTypeHeaderValue(mediaType ?? "text/plain")
			{
				CharSet = (encoding ?? Encoding.UTF8).WebName
			});
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="streamContent"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="streamContent">The response stream.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, Stream streamContent)
			where TResult : IResponseResult
		{
			return responds.Respond(HttpStatusCode.OK, streamContent);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="streamContent"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="streamContent">The response stream.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, Stream streamContent, string mediaType)
			where TResult : IResponseResult
		{
			return responds.Respond(HttpStatusCode.OK, streamContent, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="streamContent"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="streamContent">The response stream.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Stream streamContent)
			where TResult : IResponseResult
		{
			return responds.Respond(statusCode, streamContent, (MediaTypeHeaderValue)null);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="streamContent"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="streamContent">The response stream.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Stream streamContent, string mediaType)
			where TResult : IResponseResult
		{
			return responds.Respond(statusCode, streamContent, mediaType is null ? null : MediaTypeHeaderValue.Parse(mediaType));
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="streamContent"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="streamContent">The response stream.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, Stream streamContent, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			return responds.Respond(HttpStatusCode.OK, streamContent, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="streamContent"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="streamContent">The response stream.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Stream streamContent, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			if (streamContent is null)
			{
				throw new ArgumentNullException(nameof(streamContent));
			}

			if (!streamContent.CanRead)
			{
				throw new ArgumentException("Cannot read from stream.", nameof(streamContent));
			}

			byte[] buffer;
			using (var ms = new MemoryStream())
			{
				streamContent.CopyTo(ms);
				buffer = ms.ToArray();
			}

			return responds.Respond(statusCode, () => new MemoryStream(buffer), mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="streamContent"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="streamContent">The factory to create the response stream with.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Func<Stream> streamContent, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			if (responds is null)
			{
				throw new ArgumentNullException(nameof(responds));
			}

			if (streamContent is null)
			{
				throw new ArgumentNullException(nameof(streamContent));
			}

			return responds.RespondUsing(new FromStreamStrategy(statusCode, streamContent, mediaType));
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		public static T Respond<T>(this IResponds<T> responds, HttpContent content)
			where T : IResponseResult
		{
			return responds.Respond(HttpStatusCode.OK, content);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, HttpContent content)
			where TResult : IResponseResult
		{
			if (responds is null)
			{
				throw new ArgumentNullException(nameof(responds));
			}

			if (content is null)
			{
				throw new ArgumentNullException(nameof(content));
			}

			return responds.Respond(async () => new HttpResponseMessage(statusCode)
			{
				Content = await content.CloneAsByteArrayContentAsync().ConfigureAwait(false)
			});
		}

		/// <summary>
		/// Specifies to throw a <see cref="TaskCanceledException"/> simulating a HTTP request timeout.
		/// </summary>
		/// <param name="responds"></param>
		public static TResult TimesOut<TResult>(this IResponds<TResult> responds)
			where TResult : IResponseResult
		{
			return responds.TimesOutAfter(0);
		}

		/// <summary>
		/// Specifies to throw a <see cref="TaskCanceledException"/> after a specified amount of time, simulating a HTTP request timeout.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="timeoutAfterMilliseconds">The number of milliseconds after which the timeout occurs.</param>
		public static TResult TimesOutAfter<TResult>(this IResponds<TResult> responds, int timeoutAfterMilliseconds)
			where TResult : IResponseResult
		{
			return responds.TimesOutAfter(TimeSpan.FromMilliseconds(timeoutAfterMilliseconds));
		}

		/// <summary>
		/// Specifies to throw a <see cref="TaskCanceledException"/> after a specified amount of time, simulating a HTTP request timeout.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="timeoutAfter">The time after which the timeout occurs.</param>
		public static TResult TimesOutAfter<TResult>(this IResponds<TResult> responds, TimeSpan timeoutAfter)
			where TResult : IResponseResult
		{
			if (responds is null)
			{
				throw new ArgumentNullException(nameof(responds));
			}

			return responds.RespondUsing(new TimeoutStrategy(timeoutAfter));
		}
	}
}
