using System.Text;
using MockHttp.FluentAssertions;
using MockHttp.Http;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ContentTypeWithEncodingSpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body("<b>Text</b>")
            .ContentType(MediaTypes.Html, Encoding.Unicode);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Should().HaveContentType(MediaTypes.Html, Encoding.Unicode);
        return Task.CompletedTask;
    }
}
