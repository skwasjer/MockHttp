using System.Text;
using MockHttp.FluentAssertions;
using MockHttp.Http;
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
            .HaveContentType(MediaTypes.PlainText, Encoding.Unicode)
            .And.HaveContentAsync("my text", Encoding.Unicode);
    }
}
