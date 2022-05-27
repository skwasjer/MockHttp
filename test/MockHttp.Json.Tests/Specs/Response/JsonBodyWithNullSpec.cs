using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Json.Specs.Response;

public class JsonBodyWithNullSpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.JsonBody((object?)null);
    }

    protected override async Task Should(HttpResponseMessage response)
    {
        await base.Should(response);
        (await response.Should()
            .HaveContentAsync("null"))
            .And.HaveContentType("application/json; charset=utf-8");
    }
}
