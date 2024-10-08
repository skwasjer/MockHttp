﻿using System.Net.Http.Headers;

namespace MockHttp.Http;

public class HttpHeadersCollectionTests
{
    [Fact]
    public void Given_null_header_when_parsing_should_throw()
    {
        string? headers = null;

        // Act
        Action act = () => HttpHeadersCollection.Parse(headers!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(headers));
    }

    [Fact]
    public void Given_empty_header_when_parsing_should_return_empty_dictionary()
    {
        // Act
        HttpHeaders actual = HttpHeadersCollection.Parse(string.Empty);

        // Assert
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineData("key:single value", "key", "single value")]
    [InlineData("key:value1,value2", "key", "value1", "value2")]
    [InlineData("allow-spaces-in-field-value: value1 , value2 ", "allow-spaces-in-field-value", "value1", "value2")]
    public void Given_single_header_string_when_parsing_should_return_key_with_one_or_more_values(string headerString, string expectedHeaderKey, params string[] expectedValues)
    {
        // Act
        HttpHeaders headers = HttpHeadersCollection.Parse(headerString);

        // Assert
        headers.TryGetValues(expectedHeaderKey, out IEnumerable<string>? values)
            .Should()
            .BeTrue();
        values
            .Should()
            .BeEquivalentTo(expectedValues);
    }

    [Theory]
    [InlineData("no-separator", "The value cannot be null or empty.*")]
#if NET8_0_OR_GREATER
    [InlineData(":value", "The value cannot be an empty string or composed entirely of whitespace.*")]
    [InlineData(" :value", "The value cannot be an empty string or composed entirely of whitespace.*")]
    [InlineData(" leading-whitespace-not-allowed:value", "The header name ' leading-whitespace-not-allowed' has an invalid format.")]
    [InlineData("\tleading-whitespace-not-allowed:value", "The header name '\tleading-whitespace-not-allowed' has an invalid format.")]
    [InlineData("trailing-whitespace-not-allowed :value", "The header name 'trailing-whitespace-not-allowed ' has an invalid format.")]
    [InlineData("trailing-whitespace-not-allowed\t:value", "The header name 'trailing-whitespace-not-allowed\t' has an invalid format.")]
#else
#if TEST_NETSTANDARD2_1
    [InlineData(":value", "The value cannot be null or empty.*")]
    [InlineData(" :value", "The header name ' ' has an invalid format.")]
    [InlineData(" leading-whitespace-not-allowed:value", "The header name ' leading-whitespace-not-allowed' has an invalid format.")]
    [InlineData("\tleading-whitespace-not-allowed:value", "The header name '\tleading-whitespace-not-allowed' has an invalid format.")]
    [InlineData("trailing-whitespace-not-allowed :value", "The header name 'trailing-whitespace-not-allowed ' has an invalid format.")]
    [InlineData("trailing-whitespace-not-allowed\t:value", "The header name 'trailing-whitespace-not-allowed\t' has an invalid format.")]
#else
    [InlineData(":value", "The value cannot be null or empty.*")]
    [InlineData(" :value", "The header name format is invalid.")]
    [InlineData(" leading-whitespace-not-allowed:value", "The header name format is invalid.")]
    [InlineData("\tleading-whitespace-not-allowed:value", "The header name format is invalid.")]
    [InlineData("trailing-whitespace-not-allowed :value", "The header name format is invalid.")]
    [InlineData("trailing-whitespace-not-allowed\t:value", "The header name format is invalid.")]
#endif
#endif
    public void Given_single_invalid_header_string_with_when_parsing_should_throw(string invalidHeaderString, string exceptionMessage)
    {
        // Act
        Action act = () => HttpHeadersCollection.Parse(invalidHeaderString);

        // Assert
        act.Should()
            .Throw<Exception>()
            .WithMessage(exceptionMessage);
    }

    [Fact]
    public void Given_multiple_empty_newlines_when_parsing_should_ignore_newlines()
    {
        string headers = Environment.NewLine + Environment.NewLine + "Header: Value";

        // Act
        HttpHeaders actual = HttpHeadersCollection.Parse(headers);

        // Assert
        actual.Should().HaveCount(1);
        actual.Should().Contain(h => h.Key == "Header");
    }
}
