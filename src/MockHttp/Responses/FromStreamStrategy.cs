using System.Net;
using System.Net.Http.Headers;

namespace MockHttp.Responses
{
	/// <summary>
	/// Strategy that buffers a stream to a byte array, and serves responses with the byte array as content.
	/// </summary>
	internal sealed class FromStreamStrategy : IResponseStrategy
	{
		private readonly Func<Stream> _content;
		private readonly HttpStatusCode _statusCode;
		private readonly MediaTypeHeaderValue _mediaType;

		public FromStreamStrategy(HttpStatusCode statusCode, Func<Stream> content, MediaTypeHeaderValue mediaType)
		{
			_content = content ?? throw new ArgumentNullException(nameof(content));
			_statusCode = statusCode;
			_mediaType = mediaType;
		}

		public Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
		{
			Stream stream = _content();
			if (!stream.CanRead)
			{
				throw new IOException("Cannot read from stream.");
			}

			return Task.FromResult(new HttpResponseMessage(_statusCode)
			{
				Content = new StreamContent(stream)
				{
					Headers =
					{
						ContentType = _mediaType
					}
				}
			});
		}
	}
}
