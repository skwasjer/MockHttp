using System.Globalization;
using System.Net.Http.Headers;
using MockHttp.Http;
using MockHttp.Responses;

namespace MockHttp.Matchers;

public class HttpHeadersMatcherTests
{
    [Fact]
    public void Given_request_contains_expected_headers_when_matching_should_match()
    {
        DateTimeOffset lastModified = DateTimeOffset.UtcNow;
        HttpRequestMessage request = GetRequestWithHeaders(lastModified);

        var sut = new HttpHeadersMatcher(new Dictionary<string, IEnumerable<string>>
        {
            { "Cache-Control", new[] { "must-revalidate", "public", "max-age=31536000" } },
            { "Accept", new[] { MediaTypes.Json } },
            { "Last-Modified", new[] { lastModified.ToString("R", CultureInfo.InvariantCulture) } },
            { "Content-Length", new[] { "123" } }
        });

        // Act & assert
        sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Fact]
    public void Given_request_contains_expected_header_with_multiple_values_when_matching_for_single_value_should_match()
    {
        HttpRequestMessage request = GetRequestWithHeaders();

        var sut = new HttpHeadersMatcher("Cache-Control", "must-revalidate");

        // Act & assert
        sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Theory]
    [InlineData("must-revalidate", "public", "max-age=31536000")]
    [InlineData("public", "must-revalidate")]
    public void Given_request_contains_expected_header_with_multiple_values_when_matching_for_all_values_should_match_irregardless_of_order(params string[] values)
    {
        HttpRequestMessage request = GetRequestWithHeaders();

        var sut = new HttpHeadersMatcher("Cache-Control", values);

        // Act & assert
        sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Theory]
    [InlineData("*")]
    [InlineData("*revalidate")]
    [InlineData("must*")]
    [InlineData("*reval*")]
    public void Given_request_contains_expected_header_with_multiple_values_when_matching_with_wildcard_should_match(string value)
    {
        HttpRequestMessage request = GetRequestWithHeaders();

        var sut = new HttpHeadersMatcher("Cache-Control", value, true);

        // Act & assert
        sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Fact]
    public void Given_request_contains_expected_header_when_matching_with_header_name_only_should_match()
    {
        HttpRequestMessage request = GetRequestWithHeaders();

        var sut = new HttpHeadersMatcher("Cache-Control");

        // Act & assert
        sut.IsMatch(new MockHttpRequestContext(request)).Should().BeTrue();
    }

    [Fact]
    public void Given_null_header_name_when_creating_matcher_should_throw()
    {
        string? name = null;

        // Act
        Func<HttpHeadersMatcher> act = () => new HttpHeadersMatcher(name!, "value");

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(name));
    }

    [Fact]
    public void Given_null_header_name_for_multiple_values_when_creating_matcher_should_throw()
    {
        string? name = null;

        // Act
        Func<HttpHeadersMatcher> act = () => new HttpHeadersMatcher(name!, Enumerable.Empty<string>());

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(name));
    }

    [Fact]
    public void Given_null_headers_when_creating_matcher_should_throw()
    {
        IEnumerable<KeyValuePair<string, string>>? headers = null;

        // Act
        Func<HttpHeadersMatcher> act = () => new HttpHeadersMatcher(headers!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(headers));
    }

    [Fact]
    public void Given_null_headers_for_multiple_values_when_creating_matcher_should_throw()
    {
        IEnumerable<KeyValuePair<string, IEnumerable<string>>>? headers = null;

        // Act
        Func<HttpHeadersMatcher> act = () => new HttpHeadersMatcher(headers!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(headers));
    }

    [Fact]
    public void When_formatting_single_header_should_return_human_readable_representation()
    {
        const string expectedText = "Headers: header-name: header-value";
        var sut = new HttpHeadersMatcher("header-name", "header-value");

        // Act
        string displayText = sut.ToString();

        // Assert
        displayText.Should().Be(expectedText);
    }

    [Fact]
    public void When_formatting_multiple_headers_should_return_human_readable_representation()
    {
        string expectedText = $"Headers: {HeaderNames.ContentType}: {MediaTypes.PlainText}{Environment.NewLine}{HeaderNames.Accept}: {MediaTypes.PlainText}, {MediaTypes.Html}";
        var headers = new HttpHeadersCollection { { HeaderNames.ContentType, MediaTypes.PlainText }, { HeaderNames.Accept, new[] { MediaTypes.PlainText, MediaTypes.Html } } };
        var sut = new HttpHeadersMatcher(headers);

        // Act
        string displayText = sut.ToString();

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
                    MediaTypeWithQualityHeaderValue.Parse(MediaTypes.Json),
                    MediaTypeWithQualityHeaderValue.Parse($"{MediaTypes.Html};q=0.9")
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
        var sut = new HttpHeadersMatcher(new List<KeyValuePair<string, IEnumerable<string>>>());
        MockHttpRequestContext? requestContext = null;

        // Act
        Func<Task> act = () => sut.IsMatchAsync(requestContext!);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentNullException>()
            .WithParameterName(nameof(requestContext));
    }
}
