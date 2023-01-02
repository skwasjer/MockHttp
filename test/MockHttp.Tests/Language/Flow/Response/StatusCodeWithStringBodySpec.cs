#nullable enable
using System.Net;
using MockHttp.FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class StatusCodeWithStringBodySpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.StatusCode(HttpStatusCode.InternalServerError)
            .Body("my text");
    }

    protected override Task Should(HttpResponseMessage response)
    {
        return response.Should()
            .HaveStatusCode(HttpStatusCode.InternalServerError)
            .And.HaveContentAsync("my text");
    }
}
#nullable restore
