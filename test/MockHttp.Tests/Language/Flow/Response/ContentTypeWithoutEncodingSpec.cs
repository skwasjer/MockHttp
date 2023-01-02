using MockHttp.FluentAssertions;
using MockHttp.Http;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ContentTypeWithoutEncodingSpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body("<b>Text</b>")
            .ContentType(MediaTypes.Html);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Content.Should().NotBeNull();
        response.Should().HaveContentType(MediaTypes.Html);
        return Task.CompletedTask;
    }
}
