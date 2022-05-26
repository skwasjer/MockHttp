using FluentAssertions;
using MockHttp.Responses;
using Xunit;

namespace MockHttp.Matchers;

public class QueryStringMatcherTests
{
    private QueryStringMatcher _sut;

    [Theory]
    [InlineData("?key", "key", null)]
    [InlineData("?key=", "key", "")]
    [InlineData("?key=value", "key", "value")]
    [InlineData("?key1=value1&key2=value2", "key2", "value2")]
    [InlineData("?key=value1&key=value2", "key", "value1")]
    [InlineData("?key1=value1&%C3%A9%C3%B4x%C3%84=%24%25%5E%26*&key2=value", "éôxÄ", "$%^&*")]
    public void Given_queryString_equals_expected_queryString_when_matching_should_match(string queryString, string expectedKey, string expectedValue)
    {
        var request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/" + queryString) };

        _sut = new QueryStringMatcher(new[]
        {
            new KeyValuePair<string, IEnumerable<string>>(
                expectedKey,
                expectedValue is null
                    ? null
                    : new[] { expectedValue }
            )
        });

        // Act & assert
        _sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("?")]
    [InlineData("?key=value")]
    [InlineData("?key1=value1&key2=value2")]
    [InlineData("?key=value1&key=value2")]
    public void Given_queryString_does_not_equal_expected_queryString_when_matching_should_not_match(string queryString)
    {
        var request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/" + queryString) };

        _sut = new QueryStringMatcher(new[] { new KeyValuePair<string, IEnumerable<string>>("key_not_in_uri", null) });

        // Act & assert
        _sut.IsMatch(new MockHttpRequestContext(request)).Should().BeFalse();
    }

    [Fact]
    public void Given_queryString_and_no_expected_queryString_when_matching_should_not_match()
    {
        _sut = new QueryStringMatcher("");

        var request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/?unexpected=query") };

        // Act & assert
        _sut.IsMatch(new MockHttpRequestContext(request)).Should().BeFalse("no query string was expected");
    }

    [Fact]
    public void Given_null_queryString_when_creating_matcher_should_throw()
    {
        // Act
        Func<QueryStringMatcher> act = () => new QueryStringMatcher((string)null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("queryString");
    }

    [Fact]
    public void Given_null_parameters_when_creating_matcher_should_throw()
    {
        // Act
        Func<QueryStringMatcher> act = () => new QueryStringMatcher((IEnumerable<KeyValuePair<string, IEnumerable<string>>>)null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("parameters");
    }

    [Fact]
    public void When_formatting_should_return_human_readable_representation()
    {
        const string expectedText = "QueryString: '?key=value1'";
        _sut = new QueryStringMatcher("?key=value1");

        // Act
        string displayText = _sut.ToString();

        // Assert
        displayText.Should().Be(expectedText);
    }

    [Fact]
    public void Given_null_context_when_matching_it_should_throw()
    {
        _sut = new QueryStringMatcher(new List<KeyValuePair<string, IEnumerable<string>>>());
        MockHttpRequestContext requestContext = null;

        // Act
        // ReSharper disable once ExpressionIsAlwaysNull
        Action act = () => _sut.IsMatch(requestContext);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(requestContext));
    }
}
