using FluentAssertions;
using MockHttp.Responses;
using Xunit;

namespace MockHttp.Matchers;

public class RequestUriMatcherTests
{
    private RequestUriMatcher _sut;

    [Theory]
    [InlineData("", UriKind.Relative, "http://127.0.0.1/", true)]
    [InlineData("relative.htm", UriKind.Relative, "http://127.0.0.1/relative.htm", true)]
    [InlineData("/folder/relative.htm", UriKind.Relative, "http://127.0.0.1/relative.htm", false)]
    [InlineData("relative.htm", UriKind.Relative, "http://127.0.0.1/folder/relative.htm", false)]
    [InlineData("folder/relative.htm", UriKind.Relative, "http://127.0.0.1/folder/relative.htm", true)]
    [InlineData("/folder/relative.htm", UriKind.Relative, "http://127.0.0.1/folder/relative.htm", true)]
    [InlineData("http://127.0.0.1/absolute.htm", UriKind.Absolute, "http://127.0.0.1/absolute.htm", true)]
    [InlineData("http://127.0.0.1/absolute.htm", UriKind.Absolute, "http://127.0.0.1/folder/absolute.htm", false)]
    public void Given_uri_when_matching_should_match(string matchUri, UriKind uriKind, string requestUri, bool isMatch)
    {
        var request = new HttpRequestMessage { RequestUri = new Uri(requestUri, UriKind.Absolute) };
        _sut = new RequestUriMatcher(new Uri(matchUri, uriKind));

        // Act & assert
        _sut.IsMatch(new MockHttpRequestContext(request)).Should().Be(isMatch);
    }

    [Theory]
    [InlineData("relative.htm", true, "http://127.0.0.1/relative.htm", true)]
    [InlineData("/folder/relative.htm", true, "http://127.0.0.1/relative.htm", false)]
    [InlineData("relative.htm", true, "http://127.0.0.1/folder/relative.htm", false)]
    [InlineData("folder/relative.htm", true, "http://127.0.0.1/folder/relative.htm", true)]
    [InlineData("/folder/relative.htm", true, "http://127.0.0.1/folder/relative.htm", true)]
    [InlineData("http://127.0.0.1/absolute.htm", true, "http://127.0.0.1/absolute.htm", true)]
    [InlineData("http://127.0.0.1/absolute.htm", true, "http://127.0.0.1/folder/absolute.htm", false)]
    [InlineData("*.htm", true, "http://127.0.0.1/relative.htm", true)]
    [InlineData("*/relative.htm", true, "http://127.0.0.1/relative.htm", true)]
    [InlineData("/*/relative.htm", true, "http://127.0.0.1/folder/relative.htm", false)]
    [InlineData("/*/relative.htm", true, "http://127.0.0.1/relative.htm", false)]
    [InlineData("/folder/*.htm", true, "http://127.0.0.1/folder/relative.htm", false)]
    [InlineData("*/folder/*.htm", true, "http://127.0.0.1/folder/relative.htm", true)]
    [InlineData("/folder/*.htm", true, "http://127.0.0.1/relative.htm", false)]
    [InlineData("/*/*/relative.*", true, "http://127.0.0.1/folder1/folder2/relative.htm", false)]
    [InlineData("*/folder1/*/relative.*", true, "http://127.0.0.1/folder1/folder2/relative.htm", true)]
    [InlineData("/*/*/relative.*", true, "http://127.0.0.1/folder1/relative.htm", false)]
    [InlineData("http://127.0.0.1/*.htm", true, "http://127.0.0.1/absolute.htm", true)]
    [InlineData("http://127.0.0.1/*.htm", true, "http://127.0.0.1/folder/absolute.htm", true)]
    public void Given_uriString_when_matching_should_match(string uriString, bool hasWildcard, string requestUri, bool isMatch)
    {
        var request = new HttpRequestMessage { RequestUri = new Uri(requestUri, UriKind.Absolute) };
        _sut = new RequestUriMatcher(uriString, hasWildcard);

        // Act & assert
        _sut.IsMatch(new MockHttpRequestContext(request)).Should().Be(isMatch);
    }

    [Fact]
    public void Given_null_uri_when_creating_matcher_should_throw()
    {
        // Act
        Func<RequestUriMatcher> act = () => new RequestUriMatcher(null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("uri");
    }

    [Fact]
    public void Given_null_uriString_when_creating_matcher_should_throw()
    {
        // Act
        Func<RequestUriMatcher> act = () => new RequestUriMatcher(null, false);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("uriString");
    }

    [Fact]
    public void When_formatting_should_return_human_readable_representation()
    {
        const string expectedText = "RequestUri: '*/controller/*'";
        _sut = new RequestUriMatcher("*/controller/*");

        // Act
        string displayText = _sut.ToString();

        // Assert
        displayText.Should().Be(expectedText);
    }

    [Fact]
    public void Given_null_context_when_matching_it_should_throw()
    {
        _sut = new RequestUriMatcher("*/controller/*");
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
