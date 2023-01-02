using MockHttp.FluentAssertions;
using MockHttp.Http;

namespace MockHttp.Language.Flow.Response;

public class PartialByteBodySpec : ByteBodySpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body(Content, 4, 4);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        return response
            .Should()
            .HaveContentType(MediaTypes.OctetStream)
            .And.HaveContentAsync(new byte[] { 5, 4, 3, 2 });
    }
}
