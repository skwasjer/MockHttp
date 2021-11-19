using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace MockHttp.Json.SystemTextJson;

public class SystemTextJsonAdapterTests
{
    private readonly SystemTextJsonAdapter _sut;
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonAdapterTests()
    {
        _options = new JsonSerializerOptions();
        _sut = new SystemTextJsonAdapter(_options);
    }

    [Theory]
    [ClassData(typeof(SerializerTestCases))]
    public void Given_an_object_when_serializing_it_should_return_expected(object value, string expectedJson)
    {
        _sut.Serialize(value).Should().Be(expectedJson);
    }

    [Fact]
    public void Given_that_options_are_provided_when_serializing_it_should_honor_options()
    {
        object value = new { propertyName = "value" };
        string expectedJson = $"{{{Environment.NewLine}  \"propertyName\": \"value\"{Environment.NewLine}}}";

        var opts = new JsonSerializerOptions();
        opts.WriteIndented.Should().BeFalse("this is the default and thus same as not passing options");
        opts.WriteIndented = true;
        var sut = new SystemTextJsonAdapter(opts);

        // Act
        string actual = sut.Serialize(value);

        // Assert
        actual.Should().Be(expectedJson);
    }
}
