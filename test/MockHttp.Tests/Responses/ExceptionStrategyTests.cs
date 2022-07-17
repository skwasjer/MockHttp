using FluentAssertions;
using Xunit;

namespace MockHttp.Responses;

public class ExceptionStrategyTests
{
    [Fact]
    public void Given_null_factory_when_creating_instance_should_throw()
    {
        Func<Exception>? exceptionFactory = null;

        Func<ExceptionStrategy> act = () => new ExceptionStrategy(exceptionFactory!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(exceptionFactory));
    }

    [Fact]
    public async Task Given_factory_returns_null_exception_when_getting_response_should_throw()
    {
        var sut = new ExceptionStrategy(() => null!);

        // Act
        Func<Task> act = () => sut.ProduceResponseAsync(new MockHttpRequestContext(new HttpRequestMessage()), CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<HttpMockException>()
            .WithMessage("The configured exception cannot be null.");
    }

    [Fact]
    public async Task Given_factory_returns_exception_when_getting_response_should_throw()
    {
        var ex = new InvalidOperationException();
        var sut = new ExceptionStrategy(() => ex);

        // Act
        Func<Task> act = () => sut.ProduceResponseAsync(new MockHttpRequestContext(new HttpRequestMessage()), CancellationToken.None);

        // Assert
        (await act.Should().ThrowExactlyAsync<InvalidOperationException>())
            .Which.Should()
            .Be(ex);
    }
}
