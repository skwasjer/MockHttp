#nullable enable
using FluentAssertions;
using MockHttp.Specs;
using Moq;

namespace MockHttp.Language.Flow.Response;

public class FuncStreamBodyCannotReadSpec : GuardedResponseSpec
{
    private readonly Mock<Stream> _streamMock = new();

    protected override void Given(IResponseBuilder with)
    {
        _streamMock
            .Setup(m => m.CanRead)
            .Returns(false)
            .Verifiable();

        with.Body(() => _streamMock.Object);
    }

    protected override async Task ShouldThrow(Func<Task> act)
    {
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Cannot read from stream.*");
        _streamMock.Verify();
    }
}
#nullable restore
