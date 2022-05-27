#nullable enable
using FluentAssertions;

namespace MockHttp.Language.Flow.Response;

public class StringBodyWithNullSpec : StringBodySpec
{
    protected override void Given(IResponseBuilder with)
    {
        string? content = null;

        // Act
        Action act = () => with.Body(content!);

        // Assert
        act.Should()
            .ThrowExactly<ArgumentNullException>()
            .WithParameterName(nameof(content));
    }

    protected override Task Should(HttpResponseMessage response)
    {
        return Task.CompletedTask;
    }
}
#nullable restore
