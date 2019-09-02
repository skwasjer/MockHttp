using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Responses
{
	public class ExceptionStrategyTests
	{
		[Fact]
		public void Given_null_factory_when_creating_instance_should_throw()
		{
			Func<Exception> exceptionFactory = null;

			// ReSharper disable once ObjectCreationAsStatement
			// ReSharper disable once ExpressionIsAlwaysNull
			Action act = () => new ExceptionStrategy(exceptionFactory);

			// Assert
			act.Should()
				.Throw<ArgumentNullException>()
				.WithParamName(nameof(exceptionFactory));
		}

		[Fact]
		public void Given_factory_returns_null_exception_when_getting_response_should_throw()
		{
			var sut = new ExceptionStrategy(() => null);

			// Act
			Func<Task> act = () => sut.ProduceResponseAsync(new HttpRequestMessage(), CancellationToken.None);

			// Assert
			act.Should().Throw<HttpMockException>()
				.WithMessage("The configured exception cannot be null.");
		}

		[Fact]
		public void Given_factory_returns_exception_when_getting_response_should_throw()
		{
			var ex = new InvalidOperationException();
			var sut = new ExceptionStrategy(() => ex);

			// Act
			Func<Task> act = () => sut.ProduceResponseAsync(new HttpRequestMessage(), CancellationToken.None);

			// Assert
			act.Should().Throw<Exception>()
				.Which.Should().Be(ex);
		}
	}
}
