#nullable enable
using System.Text;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class StringBodyWithEncodingSpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body("my text", Encoding.Unicode);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        return response
            .Should()
            .HaveContentType(MediaTypes.TextPlain, Encoding.Unicode)
            .And.HaveContentAsync("my text", Encoding.Unicode);
    }
}
#nullable restore
