using System;
using System.Net.Http;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Matchers
{
	public class ExpressionMatcherTests
	{
		private ExpressionMatcher _sut;

		[Fact]
		public void Given_request_equals_expression_when_matching_should_match()
		{
			_sut = new ExpressionMatcher(_ => true);

			// Act & assert
			_sut.IsMatch(new HttpRequestMessage()).Should().BeTrue();
		}

		[Fact]
		public void Given_request_does_not_equal_expression_when_matching_should_not_match()
		{
			_sut = new ExpressionMatcher(_ => false);

			// Act & assert
			_sut.IsMatch(new HttpRequestMessage()).Should().BeFalse();
		}

		[Fact]
		public void Given_null_expression_when_creating_matcher_should_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new ExpressionMatcher(null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("expression");
		}
	}
}
