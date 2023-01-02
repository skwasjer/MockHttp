#nullable enable
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class HeaderWithoutValueSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Header("X-Header", (string?[])null!);
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentException>()
            .WithParameterName("headers")
            .WithMessage("At least one header must be specified.*");
    }
}
#nullable restore
