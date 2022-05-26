#nullable enable
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ContentTypeWithoutEncodingSpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body("<b>Text</b>")
            .ContentType("text/html");
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Content.Should().NotBeNull();
        response.Should().HaveContentType("text/html");
        return Task.CompletedTask;
    }
}
#nullable restore
