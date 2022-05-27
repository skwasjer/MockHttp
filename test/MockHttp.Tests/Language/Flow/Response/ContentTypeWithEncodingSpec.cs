#nullable enable
using System.Text;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ContentTypeWithEncodingSpec : ResponseSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body("<b>Text</b>")
            .ContentType("text/html", Encoding.Unicode);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Should().HaveContentType("text/html; charset=utf-16");
        return Task.CompletedTask;
    }
}
#nullable restore
