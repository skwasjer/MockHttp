using FluentAssertions;
using Xunit;

namespace MockHttp.Http;

public class QueryStringTests
{
    [Fact]
    public void Given_null_queryString_when_creating_should_throw()
    {
        IEnumerable<KeyValuePair<string, string>>? values = null;

        // Act
        Func<QueryString> act = () => new QueryString(values!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(values));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Given_queryString_with_null_or_empty_key_when_creating_should_throw(string key)
    {
        // Act
        Func<QueryString> act = () => new QueryString(new[] { new KeyValuePair<string, IEnumerable<string>>(key, new List<string>()) });

        // Assert
        act.Should()
            .Throw<FormatException>()
            .WithMessage("Key can not be null or empty.");
    }

    [Fact]
    public void Given_null_queryString_when_parsing_should_throw()
    {
        string? queryString = null;

        // Act
        Action act = () => QueryString.Parse(queryString!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(queryString));
    }

    [Fact]
    public void Given_empty_queryString_when_parsing_should_return_empty_dictionary()
    {
        // Act
        var actual = QueryString.Parse(string.Empty);

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void Given_queryString_starts_with_questionMark_when_parsing_should_ignore_it()
    {
        var expected = new Dictionary<string, IEnumerable<string>> { { "key", new[] { "value" } } };

        // Act
        var actual = QueryString.Parse("?key=value");

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Given_queryString_contains_hashTerminator_when_parsing_should_ignore_it()
    {
        var expected = new Dictionary<string, IEnumerable<string>> { { "key", new[] { "value" } } };

        // Act
        var actual = QueryString.Parse("key=value#hash");

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Given_queryString_contains_encoded_key_or_value_when_parsing_should_url_decode()
    {
        var expected = new Dictionary<string, IEnumerable<string>> { { "éôxÄ", new[] { "$%^&*" } } };

        // Act
        var actual = QueryString.Parse("%C3%A9%C3%B4x%C3%84=%24%25%5E%26*");

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Given_queryString_contains_multiple_same_keys_when_parsing_should_combine_into_single_entry()
    {
        var expected = new Dictionary<string, IEnumerable<string>> { { "key", new[] { "value", "$%^ &*", "another value" } } };

        // Act
        var actual = QueryString.Parse("?key=value&key=%24%25%5E%20%26%2A&key=another%20value");

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Given_queryString_contains_multiple_same_keys_when_formatting_should_produce_correct_string()
    {
        var queryStringData = new Dictionary<string, IEnumerable<string>> { { "key", new[] { "value", "$%^&*", "another value" } } };
        const string expected = "?key=value&key=%24%25%5E%26%2A&key=another%20value";
        var queryString = new QueryString(queryStringData);

        // Act
        string actual = queryString.ToString();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Given_queryString_contains_variation_of_keys_with_or_without_value_when_parsing_should_return_keys_with_correct_values()
    {
        const string queryString = "?key1&key2=value&key3=&key4=value1&key4=value2";
        var expected = new Dictionary<string, IEnumerable<string>>
        {
            { "key1", Array.Empty<string>() },
            { "key2", new[] { "value" } },
            { "key3", new[] { "" } },
            { "key4", new[] { "value1", "value2" } },
        };

        // Act
        var actual = QueryString.Parse(queryString);

        // Assert
        actual.Should().BeEquivalentTo(expected);
        actual.ToString().Should().BeEquivalentTo(queryString);
    }

    [Theory]
    [InlineData("http://0.0.0.0/controller/action/?query=string")]
    [InlineData("/controller/action/?query=string")]
    [InlineData("/controller/action/?query=string#ignore")]
    public void Given_uri_with_queryString_when_parsing_should_ignore_path_part(string uri)
    {
        const string expectedQueryString = "?query=string";

        // Act
        var actual = QueryString.Parse(uri);

        // Assert
        actual.ToString().Should().BeEquivalentTo(expectedQueryString);
    }

    [Fact]
    public void Given_empty_queryString_when_formatting_should_return_empty_string()
    {
        // Act
        var actual = QueryString.Parse("");

        // Assert
        actual.ToString().Should().BeEmpty();
    }

    [Fact]
    public void Given_null_key_when_parsing_should_throw()
    {
        // Act
        Action act = () => QueryString.Parse("?=value");

        // Assert
        act.Should()
            .Throw<FormatException>()
            .WithMessage("Key can not be null or empty.");
    }

    [Fact]
    public void Given_invalid_query_string_format_when_parsing_should_throw()
    {
        // Act
        Action act = () => QueryString.Parse("?key=value=another-value");

        // Assert
        act.Should()
            .Throw<FormatException>()
            .WithMessage("The escaped data string format is invalid.");
    }
}
