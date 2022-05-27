#nullable enable
using System.Net;
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class StatusCodeWithReasonPhrase : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.StatusCode(HttpStatusCode.BadRequest, "Oops!");
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        response.ReasonPhrase.Should().Be("Oops!");
        return Task.CompletedTask;
    }
}

public class StatusCodeInt32WithReasonPhrase : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.StatusCode(400, "Oops!");
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        response.ReasonPhrase.Should().Be("Oops!");
        return Task.CompletedTask;
    }
}
#nullable restore
