using System.Text.Json;

namespace MockHttp.Json.SystemTextJson;

public class SystemTextJsonEqualityComparerTests
{
    [Theory]
    [ClassData(typeof(JsonMatchTestCases))]
    public void Given_that_source_json_is_same_as_expected_when_matching_it_should_return_true(string left, string right)
    {
        var sut = new SystemTextJsonEqualityComparer(null);
        sut.Equals(left, right).Should().BeTrue();
    }

    [Theory]
    [ClassData(typeof(JsonNotMatchTestCases))]
    public void Given_that_source_json_is_not_the_same_as_expected_when_matching_it_should_return_false(string left, string right)
    {
        var sut = new SystemTextJsonEqualityComparer(null);
        sut.Equals(left, right).Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Given_that_casing_is_set_to_be_ignored_when_matching_json_with_different_case_properties_it_should_match(bool ignoresCase)
    {
        const string json = "{\"CaSe\": true}";
        const string otherJson = "{\"cAsE\": true}";
        var sut = new SystemTextJsonEqualityComparer(new JsonSerializerOptions { PropertyNameCaseInsensitive = ignoresCase });

        sut.Equals(json, otherJson).Should().Be(ignoresCase);
    }
}
