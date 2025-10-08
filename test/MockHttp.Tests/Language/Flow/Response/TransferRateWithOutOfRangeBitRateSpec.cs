using MockHttp.IO;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public sealed class TransferRateWithOutOfRangeBitRateSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body([])
            .TransferRate(RateLimitedStream.MinBitRate - 1);
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentOutOfRangeException>()
            .WithParameterName("bitRate");
    }
}
