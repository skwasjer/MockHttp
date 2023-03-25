using System.Diagnostics;

namespace MockHttp.Responses;

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
        var next = new Mock<ResponseHandlerDelegate>();

        // Act
        Func<Task> act = () => sut.HandleAsync(new MockHttpRequestContext(new HttpRequestMessage()), new HttpResponseMessage(), next.Object, CancellationToken.None);

        // Assert
        sw.Start();

#if NET5_0_OR_GREATER
        await act.Should().ThrowAsync<TaskCanceledException>().WithInnerException(typeof(TimeoutException), "the timeout expired");
#else
        await act.Should().ThrowAsync<TaskCanceledException>("the timeout expired");
#endif

        sw.Elapsed.Should().BeGreaterThanOrEqualTo(timeout);
        next.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Given_that_cancellation_token_is_cancelled_when_sending_it_should_throw()
    {
        var timeout = TimeSpan.FromSeconds(60);
        var sut = new TimeoutBehavior(timeout);
        var ct = new CancellationToken(true);
        var next = new Mock<ResponseHandlerDelegate>();

        // Act
        var sw = Stopwatch.StartNew();
        Func<Task> act = () => sut.HandleAsync(new MockHttpRequestContext(new HttpRequestMessage()), new HttpResponseMessage(), next.Object, ct);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>().Where(ex => ex.InnerException == null);
        sw.Elapsed.Should().BeLessThan(timeout);
        next.VerifyNoOtherCalls();
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
