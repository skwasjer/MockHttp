using System.Text;
using MockHttp.Response;

namespace MockHttp.Matchers;

public class PartialContentMatcherTests
{
    [Theory]
    [InlineData("request content", "utf-8")]
    [InlineData("pasākumi", "utf-16")]
    public async Task Given_request_content_equals_expected_content_when_matching_should_match(string content, string encoding)
    {
        // ReSharper disable once InlineTemporaryVariable
        string expectedContent = content;
        var enc = Encoding.GetEncoding(encoding);

        var request = new HttpRequestMessage { Content = new StringContent(content, enc) };

        var sut = new PartialContentMatcher(expectedContent, enc);

        // Act & assert
        (await sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeTrue();
    }

    [Fact]
    public async Task Given_request_content_does_not_equal_expected_content_when_matching_should_not_match()
    {
        const string content = "some http request content";
        const string expectedContent = "expected content";

        var request = new HttpRequestMessage { Content = new StringContent(content) };

        var sut = new PartialContentMatcher(expectedContent, Encoding.UTF8);

        // Act & assert
        (await sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeFalse();
    }

    [Fact]
    public async Task Given_request_content_equals_expected_content_when_matching_twice_should_match_twice()
    {
        const string content = "some http request content";
        const string expectedContent = content;

        var request = new HttpRequestMessage { Content = new StringContent(content) };

        var sut = new PartialContentMatcher(expectedContent, Encoding.UTF8);

        // Act & assert
        var ctx = new MockHttpRequestContext(request);
        (await sut.IsMatchAsync(ctx)).Should().BeTrue();
        (await sut.IsMatchAsync(ctx)).Should().BeTrue("the content should be buffered and matchable more than once");
    }

    [Fact]
    public async Task Given_request_content_is_empty_and_expected_content_is_not_when_matching_should_not_match()
    {
        var request = new HttpRequestMessage();

        var sut = new PartialContentMatcher("some data", Encoding.UTF8);

        // Act & assert
        (await sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeFalse();
    }

    [Theory]
    [InlineData("match at beginning", "match", "utf-8")]
    [InlineData("match at end", "end", "utf-8")]
    [InlineData("match in middle", "in m", "utf-16")]
    public async Task Given_request_content_contains_expected_content_when_matching_should_match(string content, string expectedContent, string encoding)
    {
        var enc = Encoding.GetEncoding(encoding);

        var request = new HttpRequestMessage { Content = new StringContent(content, enc) };

        var sut = new PartialContentMatcher(expectedContent, enc);

        // Act & assert
        (await sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeTrue();
    }

    [Fact]
    public void Given_empty_content_string_when_creating_matcher_should_throw()
    {
        // Act
        Func<PartialContentMatcher> act = () => new PartialContentMatcher(string.Empty, Encoding.UTF8);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("content");
    }

    [Fact]
    public void Given_null_content_string_with_encoding_when_creating_matcher_should_throw()
    {
        string? content = null;

        // Act
        Func<PartialContentMatcher> act = () => new PartialContentMatcher(content!, Encoding.UTF8);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(content));
    }

    [Fact]
    public void Given_empty_content_string_with_encoding_when_creating_matcher_should_throw()
    {
        string content = string.Empty;

        // Act
        Func<PartialContentMatcher> act = () => new PartialContentMatcher(content, Encoding.UTF8);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName(nameof(content));
    }

    [Fact]
    public void Given_content_string_with_null_encoding_when_creating_matcher_should_not_throw()
    {
        Encoding? encoding = null;

        // Act
        Func<PartialContentMatcher> act = () => new PartialContentMatcher("data", encoding);

        // Assert
        act.Should().NotThrow("default encoding should be used instead");
    }

    [Fact]
    public void Given_null_content_bytes_when_creating_matcher_should_throw()
    {
        byte[]? content = null;

        // Act
        Func<PartialContentMatcher> act = () => new PartialContentMatcher(content!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(content));
    }

    [Fact]
    public void Given_empty_content_bytes_when_creating_matcher_should_throw()
    {
        // Act
        Func<PartialContentMatcher> act = () => new PartialContentMatcher(Array.Empty<byte>());

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("content");
    }

    [Theory]
    [InlineData("text response data", "PartialContent: text response data")]
    [InlineData("ByteArray", "PartialContent: [0x42,0x79,0x74,0x65,0x41,0x72,0x72,0x61,0x79]")]
    [InlineData("ByteArrays", "PartialContent: [0x42,0x79,0x74,0x65,0x41,0x72,0x72,0x61,0x79,0x73]")]
    [InlineData("ByteArrayWithBigDataGetsTruncated", "PartialContent: [0x42,0x79,0x74,0x65,0x41,0x72,0x72,0x61,0x79,0x57,...](Size = 33)")]
    public void When_formatting_should_return_human_readable_representation(string content, string expectedText)
    {
        // If theory starts with ByteArray string, we will actually act as if the content was in binary form (thus no encoding)
        PartialContentMatcher sut = content.StartsWith("ByteArray")
            ? new PartialContentMatcher(Encoding.UTF8.GetBytes(content))
            : new PartialContentMatcher(content, Encoding.UTF8);

        // Act
        string displayText = sut.ToString();

        // Assert
        displayText.Should().Be(expectedText);
    }

    [Fact]
    public async Task Given_null_context_when_matching_it_should_throw()
    {
        var sut = new PartialContentMatcher("test", Encoding.UTF8);
        MockHttpRequestContext? requestContext = null;

        // Act
        Func<Task> act = () => sut.IsMatchAsync(requestContext!);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentNullException>()
            .WithParameterName(nameof(requestContext));
    }
}
