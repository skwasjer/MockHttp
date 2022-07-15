#nullable enable
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ServerTimeoutWithOutOfRangeTimeSpanSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.ServerTimeout(TimeSpan.FromMilliseconds(-1));
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentOutOfRangeException>()
            .WithParameterName("timeoutAfter");
    }
}
#nullable restore
