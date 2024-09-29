using System.Net;
using MockHttp.IO;

namespace MockHttp.Response.Behaviors;

internal sealed class TransferRateBehavior : IResponseBehavior
{
    private readonly int _bitRate;

    public TransferRateBehavior(int bitRate)
    {
        if (bitRate < RateLimitedStream.MinBitRate)
        {
            throw new ArgumentOutOfRangeException(nameof(bitRate), $"Bit rate must be higher than or equal to {RateLimitedStream.MinBitRate}.");
        }

        _bitRate = bitRate;
    }

    public async Task HandleAsync(
        MockHttpRequestContext requestContext,
        HttpResponseMessage responseMessage,
        ResponseHandler nextHandler,
        CancellationToken cancellationToken
    )
    {
        await nextHandler(requestContext, responseMessage, cancellationToken).ConfigureAwait(false);
        responseMessage.Content = new RateLimitedHttpContent(responseMessage.Content, _bitRate);
    }

    private sealed class RateLimitedHttpContent : HttpContent
    {
        private readonly int _bitRate;
        private readonly HttpContent _originalContent;

        internal RateLimitedHttpContent(HttpContent originalContent, int bitRate)
        {
            _originalContent = originalContent;
            _bitRate = bitRate;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            Stream originalStream = await _originalContent.ReadAsStreamAsync().ConfigureAwait(false);
            var rateLimitedStream = new RateLimitedStream(originalStream, _bitRate);
#if NETSTANDARD2_1_OR_GREATER || NET8_0_OR_GREATER
            await using (originalStream)
            await using (rateLimitedStream)
#else
            using (originalStream)
            using (rateLimitedStream)
#endif
            {
                await rateLimitedStream.CopyToAsync(stream).ConfigureAwait(false);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            long? contentLength = _originalContent.Headers.ContentLength;
            length = 0;
            if (contentLength.HasValue)
            {
                length = contentLength.Value;
            }

            return contentLength.HasValue;
        }
    }
}
