using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class HeaderWithoutNameSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Header(null!, Array.Empty<string>());
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentNullException>()
            .WithParameterName("name");
    }
}
