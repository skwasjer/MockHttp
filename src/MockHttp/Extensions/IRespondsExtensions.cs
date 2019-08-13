using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MockHttp.Language;
using MockHttp.Language.Flow;
#if !NETSTANDARD1_1
using System.Net.Http.Formatting;
#endif

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
			return responds.Respond(() => Task.FromResult(response()));
		}

		/// <summary>
		/// Specifies a function that returns the response for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="response">The function that provides the response message to return for given request.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, Func<HttpRequestMessage, HttpResponseMessage> response)
			where TResult : IResponseResult
		{
			return responds.Respond(request => Task.FromResult(response(request)));
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
			if (content == null)
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
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, Stream content)
			where TResult : IResponseResult
		{
			return responds.Respond(HttpStatusCode.OK, content);
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, Stream content, string mediaType)
			where TResult : IResponseResult
		{
			return responds.Respond(HttpStatusCode.OK, content, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Stream content)
			where TResult : IResponseResult
		{
			return responds.Respond(statusCode, content, "application/octet-stream");
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Stream content, string mediaType)
			where TResult : IResponseResult
		{
			return responds.Respond(statusCode, content, mediaType == null ? null : MediaTypeHeaderValue.Parse(mediaType));
		}

		/// <summary>
		/// Specifies the <see cref="HttpStatusCode.OK"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, Stream content, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			return responds.Respond(HttpStatusCode.OK, content, mediaType);
		}

		/// <summary>
		/// Specifies the <paramref name="statusCode"/> and <paramref name="content"/> to respond with for a request.
		/// </summary>
		/// <param name="responds"></param>
		/// <param name="statusCode">The status code response for given request.</param>
		/// <param name="content">The response content.</param>
		/// <param name="mediaType">The media type.</param>
		public static TResult Respond<TResult>(this IResponds<TResult> responds, HttpStatusCode statusCode, Stream content, MediaTypeHeaderValue mediaType)
			where TResult : IResponseResult
		{
			if (content == null)
			{
				throw new ArgumentNullException(nameof(content));
			}

			if (!content.CanRead)
			{
				throw new ArgumentException("Cannot read from stream.", nameof(content));
			}

			long originalStreamPos = content.Position;
			var lockObject = new object();

			byte[] buffer = null;
			return responds.Respond(() =>
			{
				if (content.CanSeek)
				{
					content.Position = originalStreamPos;
					return new HttpResponseMessage(statusCode)
					{
						Content = new StreamContent(content)
						{
							Headers =
							{
								ContentType = mediaType
							}
						}
					};
				}

				// Stream not seekable, so we have to use internal buffer for repeated responses.
				if (buffer == null)
				{
					lock (lockObject)
					{
						// Since acquired lock, check if buffer is not set by other thread.
						if (buffer == null)
						{
							using (var ms = new MemoryStream())
							{
								content.CopyTo(ms);
								buffer = ms.ToArray();
							}
						}
					}
				}

				return new HttpResponseMessage(statusCode)
				{
					Content = new ByteArrayContent(buffer)
					{
						Headers =
						{
							ContentType = mediaType
						}
					}
				};
			});
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
			if (content == null)
			{
				throw new ArgumentNullException(nameof(content));
			}

			return responds.Respond(async () => new HttpResponseMessage(statusCode)
			{
				Content = await content.CloneAsByteArrayContentAsync().ConfigureAwait(false)
			});
		}

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
			return responds.Respond(() =>
			{
				// It is somewhat unintuitive to throw TaskCanceledException but this is what HttpClient does atm,
				// so we simulate same behavior.
				// https://github.com/dotnet/corefx/issues/20296
				return Task.Delay(timeoutAfter)
					.ContinueWith(_ =>
					{
						var tcs = new TaskCompletionSource<HttpResponseMessage>();
						tcs.TrySetCanceled();
						return tcs.Task;
					}, TaskScheduler.Current)
					.Unwrap();
			});
		}
	}
}
