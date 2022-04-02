using System.Globalization;
using System.Net.Http.Headers;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Http;
using MockHttp.Responses;
using Xunit;

namespace MockHttp.Matchers;

public class HttpHeadersMatcherTests
{
    private HttpHeadersMatcher _sut;

    [Fact]
    public void Given_request_contains_expected_headers_when_matching_should_match()
    {
        DateTimeOffset lastModified = DateTimeOffset.UtcNow;
        HttpRequestMessage request = GetRequestWithHeaders(lastModified);

        _sut = new HttpHeadersMatcher(new Dictionary<string, IEnumerable<string>>
        {
            { "Cache-Control", new[] { "must-revalidate", "public", "max-age=31536000" } },
            { "Accept", new[] { "application/json" } },
            { "Last-Modified", new[] { lastModified.ToString("R", CultureInfo.InvariantCulture) } },
            { "Content-Length", new[] { "123" } }
        });

        // Act & assert
        _sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Fact]
    public void Given_request_contains_expected_header_with_multiple_values_when_matching_for_single_value_should_match()
    {
        HttpRequestMessage request = GetRequestWithHeaders();

        _sut = new HttpHeadersMatcher("Cache-Control", "must-revalidate");

        // Act & assert
        _sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Theory]
    [InlineData("must-revalidate", "public", "max-age=31536000")]
    [InlineData("public", "must-revalidate")]
    public void Given_request_contains_expected_header_with_multiple_values_when_matching_for_all_values_should_match_irregardless_of_order(params string[] values)
    {
        HttpRequestMessage request = GetRequestWithHeaders();

        _sut = new HttpHeadersMatcher("Cache-Control", values);

        // Act & assert
        _sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Theory]
    [InlineData("*")]
    [InlineData("*revalidate")]
    [InlineData("must*")]
    [InlineData("*reval*")]
    public void Given_request_contains_expected_header_with_multiple_values_when_matching_with_wildcard_should_match(string value)
    {
        HttpRequestMessage request = GetRequestWithHeaders();

        _sut = new HttpHeadersMatcher("Cache-Control", value, true);

        // Act & assert
        _sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Fact]
    public void Given_request_contains_expected_header_when_matching_with_header_name_only_should_match()
    {
        HttpRequestMessage request = GetRequestWithHeaders();

        _sut = new HttpHeadersMatcher("Cache-Control");

        // Act & assert
        _sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Fact]
    public void Given_null_header_name_when_creating_matcher_should_throw()
    {
        // Act
        Func<HttpHeadersMatcher> act = () => new HttpHeadersMatcher(null, (string)null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("name");
    }

    [Fact]
    public void Given_null_header_name_for_multiple_values_when_creating_matcher_should_throw()
    {
        // Act
        Func<HttpHeadersMatcher> act = () => new HttpHeadersMatcher(null, null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("name");
    }

    [Fact]
    public void Given_null_headers_when_creating_matcher_should_throw()
    {
        // Act
        Func<HttpHeadersMatcher> act = () => new HttpHeadersMatcher((IEnumerable<KeyValuePair<string, string>>)null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("headers");
    }

    [Fact]
    public void Given_null_headers_for_multiple_values_when_creating_matcher_should_throw()
    {
        // Act
        Func<HttpHeadersMatcher> act = () => new HttpHeadersMatcher((IEnumerable<KeyValuePair<string, IEnumerable<string>>>)null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("headers");
    }

    [Fact]
    public void When_formatting_single_header_should_return_human_readable_representation()
    {
        const string expectedText = "Headers: header-name: header-value";
        _sut = new HttpHeadersMatcher("header-name", "header-value");

        // Act
        string displayText = _sut.ToString();

        // Assert
        displayText.Should().Be(expectedText);
    }

    [Fact]
    public void When_formatting_multiple_headers_should_return_human_readable_representation()
    {
        string expectedText = $"Headers: Content-Type: text/plain{Environment.NewLine}Accept: text/plain, text/html";
        var headers = new HttpHeadersCollection { { "Content-Type", "text/plain" }, { "Accept", new[] { "text/plain", "text/html" } } };
        _sut = new HttpHeadersMatcher(headers);

        // Act
        string displayText = _sut.ToString();

        // Assert
        displayText.Should().Be(expectedText);
    }

    private static HttpRequestMessage GetRequestWithHeaders(DateTimeOffset? lastModified = null)
    {
        var request = new HttpRequestMessage
        {
            Headers =
            {
                CacheControl = CacheControlHeaderValue.Parse("public, max-age=31536000, must-revalidate"),
                Accept =
                {
                    MediaTypeWithQualityHeaderValue.Parse("application/json"),
                    MediaTypeWithQualityHeaderValue.Parse("text/html;q=0.9")
                }
            },
            Content = new StringContent("")
            {
                Headers =
                {
                    LastModified = lastModified,
                    ContentLength = 123
                }
            }
        };
        return request;
    }

    [Fact]
    public async Task Given_null_context_when_matching_it_should_throw()
    {
        _sut = new HttpHeadersMatcher(new List<KeyValuePair<string, IEnumerable<string>>>());
        MockHttpRequestContext requestContext = null;

        // Act
        // ReSharper disable once ExpressionIsAlwaysNull
        Func<Task> act = () => _sut.IsMatchAsync(requestContext);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentNullException>()
            .WithParameterName(nameof(requestContext));
    }
}
