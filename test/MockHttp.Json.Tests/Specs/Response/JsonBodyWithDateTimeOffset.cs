using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Json.Specs.Response;

public class JsonBodyWithDateTimeOffset : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.JsonBody(new DateTimeOffset(2022, 5, 26, 10, 53, 34, 123, TimeSpan.FromHours(2)));
    }

    protected override async Task Should(HttpResponseMessage response)
    {
        await base.Should(response);
        (await response.Should()
            .HaveContentAsync("\"2022-05-26T10:53:34.123+02:00\""))
            .And.HaveContentType("application/json; charset=utf-8");
    }
}
