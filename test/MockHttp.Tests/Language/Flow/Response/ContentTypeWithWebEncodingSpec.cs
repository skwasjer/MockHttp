using MockHttp.FluentAssertions;
using MockHttp.Http;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ContentTypeWithWebEncodingSpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body("<b>Text</b>")
            .ContentType($"{MediaTypes.Html}; charset=utf-16");
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Should().HaveContentType($"{MediaTypes.Html}; charset=utf-16");
        return Task.CompletedTask;
    }
}
