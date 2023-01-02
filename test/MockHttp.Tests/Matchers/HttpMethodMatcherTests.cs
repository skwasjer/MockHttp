using MockHttp.Responses;

namespace MockHttp.Matchers;

public class HttpMethodMatcherTests
{
    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("ARBITRARY")]
    public void Given_request_method_equals_expected_method_when_matching_should_match(string httpMethod)
    {
        var request = new HttpRequestMessage { Method = new HttpMethod(httpMethod) };

        var sut = new HttpMethodMatcher(new HttpMethod(httpMethod));

        // Act & assert
        sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("ARBITRARY")]
    public void Given_request_method_does_not_equal_expected_method_when_matching_should_not_match(string httpMethod)
    {
        var request = new HttpRequestMessage { Method = new HttpMethod(httpMethod) };

        var sut = new HttpMethodMatcher(HttpMethod.Put);

        // Act & assert
        sut.IsMatch(new MockHttpRequestContext(request)).Should().BeFalse();
    }

    [Fact]
    public void Given_null_method_when_creating_matcher_should_throw()
    {
        HttpMethod? method = null;

        // Act
        Func<HttpMethodMatcher> act = () => new HttpMethodMatcher(method!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(method));
    }

    [Fact]
    public void When_formatting_should_return_human_readable_representation()
    {
        const string expectedText = "Method: OPTIONS";
        var sut = new HttpMethodMatcher(HttpMethod.Options);

        // Act
        string displayText = sut.ToString();

        // Assert
        displayText.Should().Be(expectedText);
    }

    [Fact]
    public void Given_null_context_when_matching_it_should_throw()
    {
        var sut = new HttpMethodMatcher(HttpMethod.Get);
        MockHttpRequestContext? requestContext = null;

        // Act
        Action act = () => sut.IsMatch(requestContext!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(requestContext));
    }
}
