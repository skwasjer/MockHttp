using System.Text;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Http;
using MockHttp.Specs;

namespace MockHttp.Json.Specs.Response;

public class JsonBodyWithEncodingSpec : ResponseSpec
{
    protected readonly Encoding Encoding = Encoding.Unicode;

    protected override void Given(IResponseBuilder with)
    {
        with.JsonBody(new { firstName = "John", lastName = "Doe" }, Encoding);
    }

    protected override async Task Should(HttpResponseMessage response)
    {
        await base.Should(response);
        (await response.Should()
                .HaveContentAsync("{\"firstName\":\"John\",\"lastName\":\"Doe\"}", Encoding))
            .And.HaveContentType($"{MediaTypes.Json}; charset=utf-16");
    }
}
