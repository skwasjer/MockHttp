using System.Linq.Expressions;
using MockHttp.Response;

namespace MockHttp.Matchers;

public class ExpressionMatcherTests
{
    [Fact]
    public void Given_request_equals_expression_when_matching_should_match()
    {
        var sut = new ExpressionMatcher(_ => true);

        // Act & assert
        sut.IsMatch(new MockHttpRequestContext(new HttpRequestMessage())).Should().BeTrue();
    }

    [Fact]
    public void Given_request_does_not_equal_expression_when_matching_should_not_match()
    {
        var sut = new ExpressionMatcher(_ => false);

        // Act & assert
        sut.IsMatch(new MockHttpRequestContext(new HttpRequestMessage())).Should().BeFalse();
    }

    [Fact]
    public void Given_null_expression_when_creating_matcher_should_throw()
    {
        Expression<Func<HttpRequestMessage, bool>>? expression = null;

        // Act
        Func<ExpressionMatcher> act = () => new ExpressionMatcher(expression!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(expression));
    }

    [Fact]
    public void When_formatting_should_return_human_readable_representation()
    {
        const string expectedText = "Expression: message => (message.RequestUri.ToString() == \"some-uri\")";
        var sut = new ExpressionMatcher(message => message.RequestUri!.ToString() == "some-uri");

        // Act
        string displayText = sut.ToString();

        // Assert
        displayText.Should().Be(expectedText);
    }

    [Fact]
    public void Given_null_context_when_matching_it_should_throw()
    {
        var sut = new ExpressionMatcher(_ => true);
        MockHttpRequestContext? requestContext = null;

        // Act
        Action act = () => sut.IsMatch(requestContext!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(requestContext));
    }
}
