#nullable enable
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ContentTypeWithNullSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body("")
            .ContentType(null);
    }

    protected override async Task ShouldThrow(Func<Task> act)
    {
        await act.Should()
            .ThrowExactlyAsync<ArgumentNullException>()
            .WithParameterName("mediaType");
    }
}

public class ContentTypeStringWithNullSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body("")
            .ContentType((string)null!);
    }

    protected override async Task ShouldThrow(Func<Task> act)
    {
        await act.Should()
            .ThrowExactlyAsync<ArgumentNullException>()
            .WithParameterName("mediaType");
    }
}
#nullable restore
