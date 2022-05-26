﻿using System.Net.Http.Headers;
using FluentAssertions;
using MockHttp.Responses;
using Xunit;

namespace MockHttp.Matchers;

public class MediaTypeHeaderMatcherTests
{
    private MediaTypeHeaderMatcher _sut;

    [Theory]
    [InlineData("text/plain; charset=us-ascii")]
    [InlineData("application/json; charset=utf-8")]
    [InlineData("application/octet-stream")]
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

        _sut = new MediaTypeHeaderMatcher(MediaTypeHeaderValue.Parse(headerValue));

        // Act & assert
        _sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Fact]
    public void Given_headerValue_does_not_equal_expected_headerValue_when_matching_should_not_match()
    {
        const string headerValue = "text/plain";
        const string expectedHeaderValue = "text/html";

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

        _sut = new MediaTypeHeaderMatcher(MediaTypeHeaderValue.Parse(expectedHeaderValue));

        // Act & assert
        _sut.IsMatch(new MockHttpRequestContext(request)).Should().BeFalse();
    }

    [Fact]
    public void Given_content_is_null_when_matching_should_not_throw()
    {
        _sut = new MediaTypeHeaderMatcher(MediaTypeHeaderValue.Parse("text/plain"));

        // Act
        bool? result = null;
        Action act = () => result = _sut.IsMatch(new MockHttpRequestContext(new HttpRequestMessage()));

        // Assert
        act.Should().NotThrow();
        result.Should().BeFalse();
    }

    [Fact]
    public void Given_contentType_header_is_null_when_matching_should_not_throw()
    {
        var request = new HttpRequestMessage { Content = new StringContent("") };
        _sut = new MediaTypeHeaderMatcher(MediaTypeHeaderValue.Parse("text/plain"));

        // Act
        bool? result = null;
        Action act = () => result = _sut.IsMatch(new MockHttpRequestContext(request));

        // Assert
        act.Should().NotThrow();
        result.Should().BeFalse();
    }

    [Fact]
    public void Given_null_headerValue_when_creating_matcher_should_throw()
    {
        // Act
        Func<MediaTypeHeaderMatcher> act = () => new MediaTypeHeaderMatcher(null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("headerValue");
    }

    [Fact]
    public void When_formatting_should_return_human_readable_representation()
    {
        const string expectedText = "MediaType: text/html";
        _sut = new MediaTypeHeaderMatcher(MediaTypeHeaderValue.Parse("text/html"));

        // Act
        string displayText = _sut.ToString();

        // Assert
        displayText.Should().Be(expectedText);
    }

    [Fact]
    public void Given_null_context_when_matching_it_should_throw()
    {
        _sut = new MediaTypeHeaderMatcher(new MediaTypeHeaderValue("text/plain"));
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
