#nullable enable
using System.Net;
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class StatusCodeSpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.StatusCode(HttpStatusCode.BadRequest);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        return Task.CompletedTask;
    }
}

public class StatusCodeInt32Spec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.StatusCode(400);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        return Task.CompletedTask;
    }
}
#nullable restore
