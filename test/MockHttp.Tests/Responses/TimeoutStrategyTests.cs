using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Responses
{
	public class TimeoutStrategyTests
	{
		[Theory]
		[InlineData(10)]
		[InlineData(50)]
		[InlineData(100)]
		[InlineData(1000)]
		public void Given_timeout_when_sending_should_timeout_after_time_passed(int timeoutInMilliseconds)
		{
			TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutInMilliseconds);
			var sut = new TimeoutStrategy(timeout);

			// Act
			Func<Task> act = () => sut.ProduceResponseAsync(new MockHttpRequestContext(new HttpRequestMessage()), CancellationToken.None);

			// Assert
			act.ExecutionTimeOf(func => func
					.Should()
					.Throw<TaskCanceledException>("the timeout expired")
				)
				.Should()
				// Allow 5% diff.
				.BeGreaterThan(timeout - TimeSpan.FromMilliseconds(timeoutInMilliseconds * 0.95));
		}

		[Fact]
		public void Given_cancellation_token_is_cancelled_when_sending_should_throw()
		{
			TimeSpan timeout = TimeSpan.FromSeconds(60);
			var sut = new TimeoutStrategy(timeout);
			var ct = new CancellationToken(true);

			// Act
			Stopwatch sw = Stopwatch.StartNew();
			Func<Task> act = () => sut.ProduceResponseAsync(new MockHttpRequestContext(new HttpRequestMessage()), ct);

			// Assert
			act.Should().Throw<TaskCanceledException>();
			sw.Elapsed.Should().BeLessThan(timeout);
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(-60000)]
		[InlineData(-int.MaxValue)]
		[InlineData((double)int.MaxValue + 1)]
		public void Given_invalid_timespan_when_sending_should_throw(double milliseconds)
		{
			TimeSpan invalidTimeout = TimeSpan.FromMilliseconds(milliseconds);

			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new TimeoutStrategy(invalidTimeout);

			// Assert
			act.Should().Throw<ArgumentOutOfRangeException>()
				.WithParamName("timeoutAfter");
		}
	}
}
