#nullable enable
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class StringBodySpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body("my text");
    }

    protected override Task Should(HttpResponseMessage response)
    {
        return response.Should().HaveContentAsync("my text");
    }
}
#nullable restore
