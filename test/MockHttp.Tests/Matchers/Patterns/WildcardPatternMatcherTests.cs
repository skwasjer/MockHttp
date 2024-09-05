namespace MockHttp.Matchers.Patterns;

public sealed class WildcardPatternMatcherTests
{
    [Theory]
    [InlineData("test", "^(test)$")]
    [InlineData("test*", "^(test).*")]
    [InlineData("*test", ".*(test)$")]
    [InlineData("*test*", ".*(test).*")]
    [InlineData("*test1*test2*", ".*(test1).+(test2).*")]
    [InlineData("/path?*", "^(/path\\?).*")]
    [InlineData("/path/file.jpg?q=*&p=1&m=[a,b]", "^(/path/file\\.jpg\\?q=).+(&p=1&m=\\[a,b\\])$")]
    [InlineData(".+?^$()[]{}|\\", "^(\\.\\+\\?\\^\\$\\(\\)\\[\\]\\{\\}\\|\\\\)$")]
    public void When_creating_matcher_it_should_generate_expected_regex(string wildcardPattern, string expectedRegex)
    {
        var sut = new WildcardPatternMatcher(wildcardPattern);
        sut.Regex.ToString().Should().Be(expectedRegex);
    }

    [Theory]
    [InlineData("test", "test", true)]
    [InlineData("test", "testing", false)]
    [InlineData("test*", "test", true)]
    [InlineData("test*", "testing", true)]
    [InlineData("test*", "stress testing", false)]
    [InlineData("*test", "test", true)]
    [InlineData("*test", "testing", false)]
    [InlineData("*test", "stress test", true)]
    [InlineData("*test*", "test", true)]
    [InlineData("*test*", "testing", true)]
    [InlineData("*test*", "stress testing", true)]
    [InlineData("*test*", "tes", false)]
    [InlineData("*test1*test2*", "test1 test2", true)]
    [InlineData("*test1*test2*", "test0 test1 test2 test3", true)]
    [InlineData("*test1*test2*", "test test2", false)]
    [InlineData("/path?q=*", "/path?q=1", true)]
    [InlineData("/path?q=*", "/path?v=1&q=1", false)]
    [InlineData("/path/file.jpg?q=*&p=1&m=[a,b]", "/path/file.jpg?q=search%20term&p=1&m=[a,b]", true)]
    [InlineData("/path/file.jpg?q=*&p=1&m=[a,b]", "/path/file.jpg?q=search%20term&p=1&m=(a,b)", false)]
    [InlineData("/path/file.jpg?q=*&p=*&m=*", "/path/file.jpg?q=search%20term&p=1&m=[a,b]", true)]
    public void Given_that_value_matches_pattern_when_matching_it_should_pass(string wildcardPattern, string value, bool expected)
    {
        var sut = new WildcardPatternMatcher(wildcardPattern);
        sut.IsMatch(value).Should().Be(expected);
    }
}
