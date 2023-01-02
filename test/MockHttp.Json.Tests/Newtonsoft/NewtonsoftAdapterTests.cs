using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MockHttp.Json.Newtonsoft;

public class NewtonsoftAdapterTests
{
    [Theory]
    [ClassData(typeof(SerializerTestCases))]
    public void Given_an_object_when_serializing_it_should_return_expected(object value, string expectedJson)
    {
        var sut = new NewtonsoftAdapter();

        // Act
        string actual = sut.Serialize(value);

        // Assert
        actual.Should().Be(expectedJson);
    }

    [Fact]
    public void Given_that_options_are_provided_when_serializing_it_should_honor_options()
    {
        object value = new { propertyName = "value" };
        const string expectedJson = "{\"property_name\":\"value\"}";

        var contractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() };
        var sut = new NewtonsoftAdapter(new JsonSerializerSettings { ContractResolver = contractResolver });

        // Act
        string actual = sut.Serialize(value);

        // Assert
        actual.Should().Be(expectedJson);
    }
}
