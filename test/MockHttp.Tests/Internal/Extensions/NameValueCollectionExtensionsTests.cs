using System.Collections.Specialized;

namespace MockHttp.Extensions;

public class NameValueCollectionExtensionsTests
{
    [Fact]
    public void Given_null_when_getting_as_enumerable_should_throw()
    {
        NameValueCollection? nameValueCollection = null;

        Func<object> act = () => nameValueCollection!.AsEnumerable();

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(nameValueCollection));
    }

    [Fact]
    public void Given_nameValueCollection_when_getting_as_enumerable_should_succeed()
    {
        var nameValueCollection = new NameValueCollection
        {
            { "key1", "value1" },
            { "key1", "value2" },
            { "key2", "value3" },
            { "key3", null }
        };
        var expectedKeyValuePairs = new Dictionary<string, IEnumerable<string>>
        {
            {
                "key1", ["value1", "value2"]
            },
            {
                "key2", ["value3"]
            },
            {
                "key3", []
            }
        };

        // Act
        var actual = nameValueCollection.AsEnumerable().ToList();

        // Assert
        actual.Should().BeEquivalentTo(expectedKeyValuePairs);
    }
}
