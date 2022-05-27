#nullable enable
using System.Net;
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class StatusCodeOutOfRangeSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.StatusCode((HttpStatusCode)99);
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentOutOfRangeException>()
            .WithParameterName("statusCode");
    }
}

public class StatusCodeInt32OutOfRangeSpec : GuardedResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.StatusCode(99);
    }

    protected override Task ShouldThrow(Func<Task> act)
    {
        return act.Should()
            .ThrowExactlyAsync<ArgumentOutOfRangeException>()
            .WithParameterName("statusCode");
    }
}
#nullable restore
