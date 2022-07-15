#nullable enable
#pragma warning disable S4136
using System.Net;

namespace MockHttp.Responses;

internal class EmptyContent : HttpContent
{
    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return true;
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        return Task.CompletedTask;
    }

    protected override Task<Stream> CreateContentReadStreamAsync()
    {
        return Task.FromResult(Stream.Null);
    }

#if NET5_0_OR_GREATER

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        return cancellationToken.IsCancellationRequested ? Task.FromCanceled(cancellationToken) : SerializeToStreamAsync(stream, context);
    }

    protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        // The stream should be empty.
    }

    protected override Stream CreateContentReadStream(CancellationToken cancellationToken)
    {
        return Stream.Null;
    }

    protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
    {
        return cancellationToken.IsCancellationRequested
            ? Task.FromCanceled<Stream>(cancellationToken)
            : CreateContentReadStreamAsync();
    }
#endif
}
#pragma warning restore S4136
#nullable restore
