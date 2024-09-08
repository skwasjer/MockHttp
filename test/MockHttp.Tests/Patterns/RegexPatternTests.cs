using System.Text.RegularExpressions;

namespace MockHttp.Patterns;

public sealed class RegexPatternTests
{
    [Fact]
    public void Given_that_pattern_is_null_when_creating_instance_it_should_throw()
    {
        string? pattern = null;

        // Act
        Func<RegexPattern> act = () => RegexPattern.Create(pattern!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(pattern));
    }

    [Fact]
    public void Given_that_regex_is_null_when_creating_instance_it_should_throw()
    {
        Regex? pattern = null;

        // Act
        Func<RegexPattern> act = () => RegexPattern.Create(pattern!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(pattern));
    }

    [Theory]
    [InlineData("test")]
    [InlineData("^t.+st.*")]
    public void When_creating_pattern_it_should_return_expected_instance(string regexPattern)
    {
        // Act
        var sut = RegexPattern.Create(regexPattern);

        // Assert
        sut.Value.Should()
            .Be(sut.ToString())
            .And.Be(regexPattern);
    }

    [Theory]
    [InlineData("^123$", "123", true)]
    [InlineData("^123$", "1234", false)]
    [InlineData("^123", "1234", true)]
    public void Given_that_value_matches_pattern_when_matching_it_should_pass(string regexPattern, string value, bool expected)
    {
        var sut = RegexPattern.Create(regexPattern);
        sut.IsMatch(value).Should().Be(expected);
    }

    [Fact]
    public void When_casting_to_pattern_it_should_return_expected_instance()
    {
        var sut = RegexPattern.Create("test");

        // Act
        Pattern actual = sut;

        // Assert
        actual.Value.Should().BeSameAs(sut.Value);
        actual.IsMatch.Should().BeSameAs(sut.IsMatch);
    }
}
