using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class PartialByteBodyWithCountOutOfRangeSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body(Array.Empty<byte>(), 0, 10);
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentOutOfRangeException>()
            .WithParameterName("count");
    }
}

public class PartialByteBodyWithCountLessThanZeroSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body(Array.Empty<byte>(), 0, -1);
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentOutOfRangeException>()
            .WithParameterName("count");
    }
}
