using System.Diagnostics;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Responses;

public class TimeoutStrategyTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task Given_timeout_when_sending_should_timeout_after_time_passed(int timeoutInMilliseconds)
    {
        var timeout = TimeSpan.FromMilliseconds(timeoutInMilliseconds);
        var sut = new TimeoutStrategy(timeout);
        var sw = new Stopwatch();

        // Act
        Func<Task> act = () => sut.ProduceResponseAsync(new MockHttpRequestContext(new HttpRequestMessage()), CancellationToken.None);

        // Assert
        sw.Start();
        await act.Should().ThrowAsync<TaskCanceledException>("the timeout expired");
        sw.Elapsed.Should()
            // Allow 5% diff.
            .BeGreaterThan(timeout - TimeSpan.FromMilliseconds(timeoutInMilliseconds * 0.95));
    }

    [Fact]
    public async Task Given_cancellation_token_is_cancelled_when_sending_should_throw()
    {
        var timeout = TimeSpan.FromSeconds(60);
        var sut = new TimeoutStrategy(timeout);
        var ct = new CancellationToken(true);

        // Act
        var sw = Stopwatch.StartNew();
        Func<Task> act = () => sut.ProduceResponseAsync(new MockHttpRequestContext(new HttpRequestMessage()), ct);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
        sw.Elapsed.Should().BeLessThan(timeout);
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
        Func<TimeoutStrategy> act = () => new TimeoutStrategy(invalidTimeout);

        // Assert
        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParamName("timeoutAfter");
    }
}
