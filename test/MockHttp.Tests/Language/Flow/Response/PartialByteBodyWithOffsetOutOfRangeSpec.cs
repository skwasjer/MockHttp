using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class PartialByteBodyWithOffsetOutOfRangeSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body(Array.Empty<byte>(), 10, 0);
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentOutOfRangeException>()
            .WithParameterName("offset");
    }
}

public class PartialByteBodyWithOffsetLessThanZeroSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body(Array.Empty<byte>(), -1, 0);
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentOutOfRangeException>()
            .WithParameterName("offset");
    }
}
