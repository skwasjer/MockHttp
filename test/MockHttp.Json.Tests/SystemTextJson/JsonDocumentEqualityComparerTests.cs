using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace MockHttp.Json.SystemTextJson;

public class JsonDocumentEqualityComparerTests
{
    [Theory]
    [MemberData(nameof(GetDocumentMatchTestCases))]
    public void Given_that_source_json_is_same_as_expected_when_matching_it_should_return_true(JsonDocument left, JsonDocument right)
    {
        var sut = new JsonDocumentEqualityComparer(null);

        sut.Equals(left, right).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(GetDocumentNotMatchTestCases))]
    public void Given_that_source_json_is_not_the_same_as_expected_when_matching_it_should_return_false(JsonDocument left, JsonDocument right)
    {
        var sut = new JsonDocumentEqualityComparer(null);

        sut.Equals(left, right).Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Given_that_casing_is_set_to_be_ignored_when_matching_json_with_different_case_properties_it_should_match(bool ignoresCase)
    {
        var left = JsonDocument.Parse("{\"CaSe\": true}");
        var right = JsonDocument.Parse("{\"cAsE\": true}");
        var sut = new JsonDocumentEqualityComparer(new JsonSerializerOptions { PropertyNameCaseInsensitive = ignoresCase });

        sut.Equals(left, right).Should().Be(ignoresCase);
    }

    public static IEnumerable<object?[]> GetDocumentMatchTestCases()
    {
        return new JsonMatchTestCases()
            .Where(args => !args.Any(x => x is "" or null))
            .Select(args => args
                .OfType<string>()
                .Select(x => JsonDocument.Parse(x))
                .ToArray()
            );
    }

    public static IEnumerable<object?[]> GetDocumentNotMatchTestCases()
    {
        return new JsonNotMatchTestCases()
            .Where(args => !args.Any(x => x is "" or null))
            .Select(args => args
                .OfType<string>()
                .Select(x => JsonDocument.Parse(x))
                .ToArray()
            );
    }
}
