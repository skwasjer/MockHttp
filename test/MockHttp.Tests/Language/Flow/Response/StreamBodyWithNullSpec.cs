#nullable enable
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class StreamBodyWithNullSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body((Stream)null!);
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentNullException>()
            .WithParameterName("content");
    }
}
#nullable restore
