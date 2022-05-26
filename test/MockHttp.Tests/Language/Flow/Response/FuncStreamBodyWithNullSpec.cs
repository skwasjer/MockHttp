#nullable enable
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class FuncStreamBodyWithNullSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body((Func<Stream>)null!);
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentNullException>()
            .WithParameterName("contentFactory");
    }
}
#nullable restore
