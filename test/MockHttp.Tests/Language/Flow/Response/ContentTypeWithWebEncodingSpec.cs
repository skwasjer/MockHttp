#nullable enable
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ContentTypeWithWebEncodingSpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body("<b>Text</b>")
            .ContentType("text/html; charset=utf-16");
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Should().HaveContentType("text/html; charset=utf-16");
        return Task.CompletedTask;
    }
}
#nullable restore
