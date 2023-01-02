#nullable enable
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ClientTimeoutWithOutOfRangeTimeSpanSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.ClientTimeout(TimeSpan.FromMilliseconds(-1));
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentOutOfRangeException>()
            .WithParameterName("timeoutAfter");
    }
}
#nullable restore
