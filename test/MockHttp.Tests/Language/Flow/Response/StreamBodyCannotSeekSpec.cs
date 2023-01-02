namespace MockHttp.Language.Flow.Response;

public class StreamBodyCannotSeekSpec : StreamBodySpec
{
    private Mock<MemoryStream>? _streamMock;

    protected override Stream CreateStream()
    {
        _streamMock = new Mock<MemoryStream>(Content) { CallBase = true };
        _streamMock
            .Setup(s => s.CanSeek)
            .Returns(false);
        return _streamMock.Object;
    }

    protected override Task Should(HttpResponseMessage response)
    {
        _streamMock?.Verify();
        return base.Should(response);
    }

    public override Task DisposeAsync()
    {
        _streamMock?.Object.Dispose();
        return base.DisposeAsync();
    }
}
