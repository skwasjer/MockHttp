using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MockHttp.Responses
{
	/// <summary>
	/// Strategy that buffers a stream to a byte array, and serves responses with the byte array as content.
	/// </summary>
	internal class FromStreamStrategy : IResponseStrategy
	{
		private readonly Stream _content;
		private readonly HttpStatusCode _statusCode;
		private readonly MediaTypeHeaderValue _mediaType;
		private readonly object _lockObject = new object();
		private byte[] _buffer;

		public FromStreamStrategy(HttpStatusCode statusCode, Stream content, MediaTypeHeaderValue mediaType)
		{
			_content = content ?? throw new ArgumentNullException(nameof(content));
			if (!content.CanRead)
			{
				throw new ArgumentException("Cannot read from stream.", nameof(content));
			}

			_statusCode = statusCode;
			_mediaType = mediaType;
		}

		public Task<HttpResponseMessage> ProduceResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			InitBuffer();

			return Task.FromResult(new HttpResponseMessage(_statusCode)
			{
				Content = new ByteArrayContent(_buffer)
				{
					Headers =
					{
						ContentType = _mediaType
					}
				}
			});
		}

		private void InitBuffer()
		{
			if (_buffer != null)
			{
				return;
			}

			lock (_lockObject)
			{
				if (_buffer != null)
				{
					return;
				}

				using (var ms = new MemoryStream())
				{
					_content.CopyTo(ms);
					_buffer = ms.ToArray();
				}
			}
		}
	}
}