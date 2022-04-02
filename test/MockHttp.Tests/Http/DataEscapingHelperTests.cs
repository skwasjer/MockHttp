using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Http;

public class DataEscapingHelperTests
{
    [Fact]
    public void Given_null_escapedString_when_parsing_should_throw()
    {
        string dataEscapedString = null;

        // Act
        // ReSharper disable once ExpressionIsAlwaysNull
        Action act = () => DataEscapingHelper.Parse(dataEscapedString);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(dataEscapedString));
    }

    [Fact]
    public void Given_empty_escapedString_when_parsing_should_return_empty()
    {
        // Act
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> actual = DataEscapingHelper.Parse(string.Empty);

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void Given_escapedString_contains_encoded_key_or_value_when_parsing_should_url_decode()
    {
        var expected = new Dictionary<string, IEnumerable<string>> { { "éôxÄ", new[] { "$%^&*" } } };

        // Act
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> actual = DataEscapingHelper.Parse("%C3%A9%C3%B4x%C3%84=%24%25%5E%26*");

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Given_escapedString_contains_multiple_same_keys_when_parsing_should_combine_into_single_entry()
    {
        var expected = new Dictionary<string, IEnumerable<string>> { { "key", new[] { "value", "$%^ &*", "another value" } } };

        // Act
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> actual = DataEscapingHelper.Parse("key=value&key=%24%25%5E%20%26%2A&key=another%20value");

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Given_escapedString_contains_multiple_same_keys_when_formatting_should_produce_correct_string()
    {
        var items = new Dictionary<string, IEnumerable<string>> { { "key", new[] { "value", "$%^&*", "another value" } } };
        const string expected = "key=value&key=%24%25%5E%26%2A&key=another%20value";

        // Act
        string actual = DataEscapingHelper.Format(items);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Given_escapedString_contains_variation_of_keys_with_or_without_value_when_parsing_should_return_keys_with_correct_values()
    {
        const string dataEscapedString = "key1&key2=value&key3=&key4=value1&key4=value2";
        var expected = new Dictionary<string, IEnumerable<string>>
        {
            { "key1", Array.Empty<string>() },
            { "key2", new[] { "value" } },
            { "key3", new[] { "" } },
            { "key4", new[] { "value1", "value2" } },
        };

        // Act
        var actual = DataEscapingHelper.Parse(dataEscapedString).ToList();

        // Assert
        actual.Should().BeEquivalentTo(expected);
        DataEscapingHelper.Format(actual).Should().BeEquivalentTo(dataEscapedString);
    }

    [Fact]
    public void Given_empty_escapedString_when_formatting_should_return_empty_string()
    {
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> items = DataEscapingHelper.Parse("");

        // Act
        string actual = DataEscapingHelper.Format(items);

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void Given_null_key_when_parsing_should_throw()
    {
        // Act
        Action act = () => DataEscapingHelper.Parse("=value");

        // Assert
        act.Should()
            .Throw<FormatException>()
            .WithMessage("Key can not be null or empty.");
    }

    [Fact]
    public void Given_invalid_escapedString_format_when_parsing_should_throw()
    {
        // Act
        Action act = () => DataEscapingHelper.Parse("key=value=another-value");

        // Assert
        act.Should()
            .Throw<FormatException>()
            .WithMessage("The escaped data string format is invalid.");
    }
}
