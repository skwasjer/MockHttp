using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Json.Specs.Response;

public class JsonBodyWithStringSpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.JsonBody("some text");
    }

    protected override async Task Should(HttpResponseMessage response)
    {
        await base.Should(response);
        (await response.Should()
            .HaveContentAsync("\"some text\""))
            .And.HaveContentType("application/json; charset=utf-8");
    }
}
