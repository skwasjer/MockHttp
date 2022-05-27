#nullable enable
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class HttpContentBodySpec : ResponseSpec
{
    private readonly HttpContent _content = new StringContent("text");

    protected override void Given(IResponseBuilder with)
    {
        with.Body(_content);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Content.Should().BeSameAs(_content);
        return Task.CompletedTask;
    }

    public override Task DisposeAsync()
    {
        _content.Dispose();
        return base.DisposeAsync();
    }
}
#nullable restore
