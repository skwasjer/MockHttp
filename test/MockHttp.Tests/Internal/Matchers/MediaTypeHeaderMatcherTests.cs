using System.Net.Http.Headers;
using MockHttp.Http;

namespace MockHttp.Matchers;

public class MediaTypeHeaderMatcherTests
{
    [Theory]
    [InlineData($"{MediaTypes.PlainText}; charset=us-ascii")]
    [InlineData($"{MediaTypes.Json}; charset=utf-8")]
    [InlineData(MediaTypes.OctetStream)]
    public void Given_headerValue_equals_expected_headerValue_when_matching_should_match(string headerValue)
    {
        var request = new HttpRequestMessage
        {
            Content = new StringContent("")
            {
                Headers =
                {
                    ContentType = MediaTypeHeaderValue.Parse(headerValue)
                }
            }
        };

        var sut = new MediaTypeHeaderMatcher(MediaTypeHeaderValue.Parse(headerValue));

        // Act & assert
        sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Fact]
    public void Given_headerValue_does_not_equal_expected_headerValue_when_matching_should_not_match()
    {
        const string headerValue = MediaTypes.PlainText;
        const string expectedHeaderValue = MediaTypes.Html;

        var request = new HttpRequestMessage
        {
            Content = new StringContent("")
            {
                Headers =
                {
                    ContentType = MediaTypeHeaderValue.Parse(headerValue)
                }
            }
        };

        var sut = new MediaTypeHeaderMatcher(MediaTypeHeaderValue.Parse(expectedHeaderValue));

        // Act & assert
        sut.IsMatch(new MockHttpRequestContext(request)).Should().BeFalse();
    }

    [Fact]
    public void Given_content_is_null_when_matching_should_not_throw()
    {
        var sut = new MediaTypeHeaderMatcher(MediaTypeHeaderValue.Parse(MediaTypes.PlainText));

        // Act
        bool? result = null;
        Action act = () => result = sut.IsMatch(new MockHttpRequestContext(new HttpRequestMessage()));

        // Assert
        act.Should().NotThrow();
        result.Should().BeFalse();
    }

    [Fact]
    public void Given_contentType_header_is_null_when_matching_should_not_throw()
    {
        var request = new HttpRequestMessage { Content = new StringContent("") };
        var sut = new MediaTypeHeaderMatcher(MediaTypeHeaderValue.Parse(MediaTypes.PlainText));

        // Act
        bool? result = null;
        Action act = () => result = sut.IsMatch(new MockHttpRequestContext(request));

        // Assert
        act.Should().NotThrow();
        result.Should().BeFalse();
    }

    [Fact]
    public void Given_null_headerValue_when_creating_matcher_should_throw()
    {
        MediaTypeHeaderValue? headerValue = null;

        // Act
        Func<MediaTypeHeaderMatcher> act = () => new MediaTypeHeaderMatcher(headerValue!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(headerValue));
    }

    [Fact]
    public void When_formatting_should_return_human_readable_representation()
    {
        const string expectedText = "MediaType: text/html";
        var sut = new MediaTypeHeaderMatcher(MediaTypeHeaderValue.Parse(MediaTypes.Html));

        // Act
        string displayText = sut.ToString();

        // Assert
        displayText.Should().Be(expectedText);
    }

    [Fact]
    public void Given_null_context_when_matching_it_should_throw()
    {
        var sut = new MediaTypeHeaderMatcher(new MediaTypeHeaderValue(MediaTypes.PlainText));
        MockHttpRequestContext? requestContext = null;

        // Act
        Action act = () => sut.IsMatch(requestContext!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(requestContext));
    }
}
