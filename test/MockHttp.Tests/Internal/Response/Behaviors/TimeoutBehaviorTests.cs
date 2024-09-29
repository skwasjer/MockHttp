using System.Diagnostics;

namespace MockHttp.Response.Behaviors;

public class TimeoutBehaviorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task Given_that_timeout_is_not_zero_when_sending_it_should_timeout_after_time_n_passed(int timeoutInMilliseconds)
    {
        var timeout = TimeSpan.FromMilliseconds(timeoutInMilliseconds);
        var sut = new TimeoutBehavior(timeout);
        var sw = new Stopwatch();
        ResponseHandler nextStub = Substitute.For<ResponseHandler>();

        // Act
        Func<Task> act = () => sut.HandleAsync(new MockHttpRequestContext(new HttpRequestMessage()), new HttpResponseMessage(), nextStub, CancellationToken.None);

        // Assert
        sw.Start();

#if NET5_0_OR_GREATER
        await act.Should().ThrowAsync<TaskCanceledException>().WithInnerException(typeof(TimeoutException), "the timeout expired");
#else
        await act.Should().ThrowAsync<TaskCanceledException>("the timeout expired");
#endif

        sw.Elapsed.Should().BeGreaterThanOrEqualTo(timeout);
        nextStub.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task Given_that_cancellation_token_is_cancelled_when_sending_it_should_throw()
    {
        var timeout = TimeSpan.FromSeconds(60);
        var sut = new TimeoutBehavior(timeout);
        var ct = new CancellationToken(true);
        ResponseHandler nextStub = Substitute.For<ResponseHandler>();

        // Act
        var sw = Stopwatch.StartNew();
        Func<Task> act = () => sut.HandleAsync(new MockHttpRequestContext(new HttpRequestMessage()), new HttpResponseMessage(), nextStub, ct);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>().Where(ex => ex.InnerException == null);
        sw.Elapsed.Should().BeLessThan(timeout);
        nextStub.ReceivedCalls().Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-60000)]
    [InlineData(-int.MaxValue)]
    [InlineData((double)int.MaxValue + 1)]
    public void Given_that_timeout_is_invalid_when_sending_it_should_throw(double milliseconds)
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
