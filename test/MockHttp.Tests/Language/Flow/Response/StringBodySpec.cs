#nullable enable
using System.Text;
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
        return response
            .Should()
            .HaveContentType(MediaTypes.TextPlain, Encoding.UTF8)
            .And.HaveContentAsync("my text");
    }
}
#nullable restore
