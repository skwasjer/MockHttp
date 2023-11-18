using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class FuncStreamBodyCannotReadSpec : GuardedResponseSpec
{
    private readonly Stream _streamMock = Substitute.For<Stream>();

    protected override void Given(IResponseBuilder with)
    {
        _streamMock.CanRead.Returns(false);

        with.Body(() => _streamMock);
    }

    protected override async Task ShouldThrow(Func<Task> act)
    {
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Cannot read from stream.*");
        _ = _streamMock.Received(1).CanRead;
    }
}
