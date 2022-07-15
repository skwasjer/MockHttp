#nullable enable
using FluentAssertions;
using MockHttp.FluentAssertions;

namespace MockHttp.Language.Flow.Response;

public class HttpContentBodyWithCustomHeadersSpec : HttpContentBodySpec
{
    protected override void Given(IResponseBuilder with)
    {
        Content.Headers.TryAddWithoutValidation("X-Test", "TestValue");
        base.Given(with);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Should().HaveHeader("X-Test", "TestValue");
        return base.Should(response);
    }
}
#nullable restore
