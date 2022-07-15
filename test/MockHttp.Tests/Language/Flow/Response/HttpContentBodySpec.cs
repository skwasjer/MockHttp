#nullable enable
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class HttpContentBodySpec : ResponseSpec
{
    protected readonly HttpContent Content = new StringContent("text");

    protected override void Given(IResponseBuilder with)
    {
        with.Body(Content);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Content.Should().BeSameAs(Content);
        return Task.CompletedTask;
    }

    public override Task DisposeAsync()
    {
        Content.Dispose();
        return base.DisposeAsync();
    }
}
#nullable restore
