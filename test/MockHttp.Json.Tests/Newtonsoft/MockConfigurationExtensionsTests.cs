using Newtonsoft.Json;

namespace MockHttp.Json.Newtonsoft;

public class MockConfigurationExtensionsTests
{
    private readonly MockHttpHandler _sut;

    public MockConfigurationExtensionsTests()
    {
        _sut = new MockHttpHandler();
    }

    [Fact]
    public void Given_that_config_is_null_when_using_it_should_throw()
    {
        IMockConfiguration? mockConfig = null;

#pragma warning disable CS8604
        Func<IMockConfiguration> act = () => mockConfig.UseNewtonsoftJson();
#pragma warning restore CS8604

        // Assert
        act.Should()
            .ThrowExactly<ArgumentNullException>()
            .Which.ParamName.Should()
            .Be(nameof(mockConfig));
    }

    [Fact]
    public void Given_that_settings_is_not_specified_when_using_it_should_register_adapter_without_settings()
    {
        // Act
        _sut.UseNewtonsoftJson();

        // Assert
        ((IMockConfiguration)_sut).Items
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .Should()
            .ContainKey(typeof(IJsonAdapter))
            .WhoseValue.Should()
            .BeOfType<NewtonsoftAdapter>()
            .Which.Settings.Should()
            .BeNull();
    }

    [Fact]
    public void Given_that_settings_is_specified_when_using_it_should_register_adapter_with_settings()
    {
        var settings = new JsonSerializerSettings();

        // Act
        _sut.UseNewtonsoftJson(settings);

        // Assert
        ((IMockConfiguration)_sut).Items
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .Should()
            .ContainKey(typeof(IJsonAdapter))
            .WhoseValue.Should()
            .BeOfType<NewtonsoftAdapter>()
            .Which.Settings.Should()
            .BeSameAs(settings);
    }
}
