using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Http;
using MockHttp.Specs;

namespace MockHttp.Json.Specs.Response;

public class JsonBodySpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.JsonBody(new { firstName = "John", lastName = "Doe" });
    }

    protected override async Task Should(HttpResponseMessage response)
    {
        await base.Should(response);
        (await response.Should()
            .HaveContentAsync("{\"firstName\":\"John\",\"lastName\":\"Doe\"}"))
            .And.HaveContentType($"{MediaTypes.Json}; charset=utf-8");
    }
}
