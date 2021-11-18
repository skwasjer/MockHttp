using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Responses;
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
			_sut.IsMatch(new MockHttpRequestContext(new HttpRequestMessage())).Should().BeTrue();
		}

		[Fact]
		public void Given_request_does_not_equal_expression_when_matching_should_not_match()
		{
			_sut = new ExpressionMatcher(_ => false);

			// Act & assert
			_sut.IsMatch(new MockHttpRequestContext(new HttpRequestMessage())).Should().BeFalse();
		}

		[Fact]
		public void Given_null_expression_when_creating_matcher_should_throw()
		{
			// Act
			Func<ExpressionMatcher> act = () => new ExpressionMatcher(null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("expression");
		}

		[Fact]
		public void When_formatting_should_return_human_readable_representation()
		{
			const string expectedText = "Expression: message => (message.RequestUri.ToString() == \"some-uri\")";
			_sut = new ExpressionMatcher(message => message.RequestUri.ToString() == "some-uri");

			// Act
			string displayText = _sut.ToString();

			// Assert
			displayText.Should().Be(expectedText);
		}

		[Fact]
		public void Given_null_context_when_matching_it_should_throw()
		{
			_sut = new ExpressionMatcher(_ => true);
			MockHttpRequestContext requestContext = null;

			// Act
			// ReSharper disable once ExpressionIsAlwaysNull
			Action act = () => _sut.IsMatch(requestContext);

			// Assert
			act.Should()
				.Throw<ArgumentNullException>()
				.WithParamName(nameof(requestContext));
		}
	}
}
