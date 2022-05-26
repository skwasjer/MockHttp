#nullable enable
using FluentAssertions;
using MockHttp.FluentAssertions;

namespace MockHttp.Language.Flow.Response;

public class StreamBodySpec : ByteBodySpec
{
    private Stream? _stream;

    protected override void Given(IResponseBuilder with)
    {
        _stream?.Dispose();
        _stream = CreateStream();
        with.Body(_stream);
    }

    protected virtual Stream CreateStream()
    {
        return new MemoryStream(Content);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        return response.Should().HaveContentAsync(Content);
    }

    public override Task DisposeAsync()
    {
        _stream?.Dispose();
        return base.DisposeAsync();
    }
}
#nullable restore
