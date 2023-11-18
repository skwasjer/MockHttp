namespace MockHttp.Language.Flow.Response;

public class StreamBodyCannotSeekSpec : StreamBodySpec
{
    private MemoryStream? _streamMock;

    protected override Stream CreateStream()
    {
        _streamMock = Substitute.ForPartsOf<CanSeekMemoryStream>(Content, false);
        return _streamMock;
    }

    protected override Task Should(HttpResponseMessage response)
    {
        _ = _streamMock!.Received().CanSeek;
        return base.Should(response);
    }

    public override Task DisposeAsync()
    {
        _streamMock?.Dispose();
        return base.DisposeAsync();
    }
}
