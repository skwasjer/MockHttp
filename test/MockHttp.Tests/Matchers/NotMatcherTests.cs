using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Responses;
using Moq;
using Xunit;

namespace MockHttp.Matchers;

public class NotMatcherTests
{
	private readonly Mock<IAsyncHttpRequestMatcher> _innerMatcherMock;
	private readonly NotMatcher _sut;

	public NotMatcherTests()
	{
		_innerMatcherMock = new Mock<IAsyncHttpRequestMatcher>();
		_sut = new NotMatcher(_innerMatcherMock.Object);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Given_inner_matcher_when_matching_it_should_return_inverse(bool innerMatchResult)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, "http://0.0.0.0/url");
		_innerMatcherMock
			.Setup(m => m.IsMatchAsync(It.IsAny<MockHttpRequestContext>()))
			.ReturnsAsync(innerMatchResult);

		// Act & assert
		(await _sut.IsMatchAsync(new MockHttpRequestContext(request)))
			.Should()
			.Be(!innerMatchResult);
	}

	[Fact]
	public void Given_null_inner_matcher_when_creating_matcher_should_throw()
	{
		// Act
		Func<NotMatcher> act = () => new NotMatcher(null);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParamName("matcher");
	}

	[Fact]
	public void When_formatting_should_return_human_readable_representation()
	{
		const string innerMatcherText = "Type: text";
		string expectedText = $"Not {innerMatcherText}";
		_innerMatcherMock.Setup(m => m.ToString()).Returns(innerMatcherText);

		// Act
		string displayText = _sut.ToString();

		// Assert
		displayText.Should().Be(expectedText);
	}

	[Fact]
	public async Task Given_null_context_when_matching_it_should_throw()
	{
		MockHttpRequestContext requestContext = null;

		// Act
		// ReSharper disable once ExpressionIsAlwaysNull
		Func<Task> act = () => _sut.IsMatchAsync(requestContext);

		// Assert
		await act.Should()
			.ThrowAsync<ArgumentNullException>()
			.WithParamName(nameof(requestContext));
	}
}