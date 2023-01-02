namespace MockHttp.Json;

public class MockConfigurationExtensionsTests
{
    private readonly MockHttpHandler _sut;

    public MockConfigurationExtensionsTests()
    {
        _sut = new MockHttpHandler();
    }

    [Theory]
    [MemberData(nameof(UseJsonAdapterNullTestCases))]
    public void Given_that_arg_is_null_when_creating_instance_using_it_should_throw
    (
        IMockConfiguration mockConfig,
        IJsonAdapter jsonAdapter,
        string expectedParamName)
    {
        // Act
        Func<IMockConfiguration> act = () => mockConfig.UseJsonAdapter(jsonAdapter);

        // Assert
        act.Should()
            .ThrowExactly<ArgumentNullException>()
            .Which.ParamName.Should()
            .Be(expectedParamName);
    }

    public static IEnumerable<object?[]> UseJsonAdapterNullTestCases()
    {
        IMockConfiguration mockConfig = Mock.Of<IMockConfiguration>();
        IJsonAdapter jsonAdapter = Mock.Of<IJsonAdapter>();

        yield return new object?[] { null, jsonAdapter, nameof(mockConfig) };
        yield return new object?[] { mockConfig, null, nameof(jsonAdapter) };
    }

    [Fact]
    public void When_using_it_should_register_adapter()
    {
        IJsonAdapter adapter = Mock.Of<IJsonAdapter>();

        // Act
        _sut.UseJsonAdapter(adapter);

        // Assert
        ((IMockConfiguration)_sut).Items
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .Should()
            .ContainKey(typeof(IJsonAdapter))
            .WhoseValue.Should()
            .BeSameAs(adapter);
    }
}
