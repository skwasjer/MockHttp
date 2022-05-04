using System.Diagnostics;
using FluentAssertions;
using Moq;
using Xunit;

namespace MockHttp.Responses;

public class TimeoutBehaviorTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task Given_timeout_when_sending_should_timeout_after_time_passed(int timeoutInMilliseconds)
    {
        var timeout = TimeSpan.FromMilliseconds(timeoutInMilliseconds);
        var sut = new TimeoutBehavior(timeout);
        var sw = new Stopwatch();
        var next = new Mock<ResponseHandlerDelegate>();

        // Act
        Func<Task> act = () => sut.HandleAsync(new MockHttpRequestContext(new HttpRequestMessage()), new HttpResponseMessage(), next.Object, CancellationToken.None);

        // Assert
        sw.Start();
        await act.Should().ThrowAsync<TaskCanceledException>("the timeout expired");
        sw.Elapsed.Should()
            // Allow 5% diff.
            .BeGreaterThan(timeout - TimeSpan.FromMilliseconds(timeoutInMilliseconds * 0.95));
        next.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Given_cancellation_token_is_cancelled_when_sending_should_throw()
    {
        var timeout = TimeSpan.FromSeconds(60);
        var sut = new TimeoutBehavior(timeout);
        var ct = new CancellationToken(true);
        var next = new Mock<ResponseHandlerDelegate>();

        // Act
        var sw = Stopwatch.StartNew();
        Func<Task> act = () => sut.HandleAsync(new MockHttpRequestContext(new HttpRequestMessage()), new HttpResponseMessage(), next.Object, ct);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
        sw.Elapsed.Should().BeLessThan(timeout);
        next.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-60000)]
    [InlineData(-int.MaxValue)]
    [InlineData((double)int.MaxValue + 1)]
    public void Given_invalid_timespan_when_sending_should_throw(double milliseconds)
    {
        var invalidTimeout = TimeSpan.FromMilliseconds(milliseconds);

        // Act
        Func<TimeoutBehavior> act = () => new TimeoutBehavior(invalidTimeout);

        // Assert
        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("timeoutAfter");
    }
}
