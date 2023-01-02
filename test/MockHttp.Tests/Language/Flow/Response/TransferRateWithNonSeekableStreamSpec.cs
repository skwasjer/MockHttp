#nullable enable
using MockHttp.IO;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class TransferRateWithNonSeekableStreamSpec
    : GuardedResponseSpec
{
    private readonly RateLimitedStream _nonSeekableStream;

    public TransferRateWithNonSeekableStreamSpec()
    {
        _nonSeekableStream = new RateLimitedStream(new CanSeekMemoryStream(Array.Empty<byte>(), false), 1000);
    }

    public override Task DisposeAsync()
    {
        _nonSeekableStream.Dispose();
        return base.DisposeAsync();
    }

    protected override void Given(IResponseBuilder with)
    {
        with.Body(_nonSeekableStream);
    }

    protected override async Task ShouldThrow(Func<Task> act)
    {
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Cannot use a rate limited stream that is not seekable.*");
    }
}
#nullable restore
