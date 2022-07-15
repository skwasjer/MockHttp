#nullable enable
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ByteBodySpec : ResponseSpec
{
    protected byte[] Content { get; set; } = { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, byte.MaxValue };

    protected override void Given(IResponseBuilder with)
    {
        with.Body(Content);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        return response
            .Should()
            .HaveContentType(MediaTypes.OctetStream)
            .And.HaveContentAsync(Content);
    }
}
#nullable restore
