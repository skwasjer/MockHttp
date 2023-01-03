namespace MockHttp.Language.Flow.Response;

public class FuncStreamBodyOutsideClosureSpec : FuncStreamBodySpec
{
    private CanSeekMemoryStream? _stream;

    protected override void Given(IResponseBuilder with)
    {
        _stream?.Dispose();
        _stream = new CanSeekMemoryStream(Content, false);
        with.Body(() => _stream);
    }

    protected override async Task<HttpResponseMessage> When(HttpClient httpClient)
    {
        HttpResponseMessage response1 = await base.When(httpClient);

        Func<Task> secondRequest = () => base.Send(httpClient);
        await secondRequest.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot read from stream.", because: "the stream was not seekable and thus disposed after first request");

        return response1;
    }

    public override Task DisposeAsync()
    {
        _stream?.Dispose();
        return base.DisposeAsync();
    }
}
