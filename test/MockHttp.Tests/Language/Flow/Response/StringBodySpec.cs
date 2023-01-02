using System.Text;
using MockHttp.FluentAssertions;
using MockHttp.Http;
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
            .HaveContentType(MediaTypes.PlainText, Encoding.UTF8)
            .And.HaveContentAsync("my text");
    }
}
