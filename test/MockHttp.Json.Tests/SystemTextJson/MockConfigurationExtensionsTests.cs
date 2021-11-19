using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace MockHttp.Json.SystemTextJson;

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
        Func<IMockConfiguration> act = () => mockConfig.UseSystemTextJson();
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
        _sut.UseSystemTextJson();

        // Assert
        ((IMockConfiguration)_sut).Items
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .Should()
            .ContainKey(typeof(IJsonAdapter))
            .WhoseValue.Should()
            .BeOfType<SystemTextJsonAdapter>()
            .Which.Options.Should()
            .BeNull();
    }

    [Fact]
    public void Given_that_settings_is_specified_when_using_it_should_register_adapter_with_settings()
    {
        var options = new JsonSerializerOptions();

        // Act
        _sut.UseSystemTextJson(options);

        // Assert
        ((IMockConfiguration)_sut).Items
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .Should()
            .ContainKey(typeof(IJsonAdapter))
            .WhoseValue.Should()
            .BeOfType<SystemTextJsonAdapter>()
            .Which.Options.Should()
            .BeSameAs(options);
    }
}
