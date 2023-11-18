using MockHttp.Json.SystemTextJson;
using MockHttp.Responses;

namespace MockHttp.Json.Extensions;

public class MockHttpRequestContextExtensionsTests
{
    [Fact]
    public void Given_that_adapter_is_not_registered_when_getting_adapter_it_should_return_newtonsoft_by_default()
    {
        using var msg = new HttpRequestMessage();
        var sut = new MockHttpRequestContext(msg);
        sut.Services.Should().BeEmpty();

        // Act
        IJsonAdapter actual = sut.GetAdapter();

        // Assert
        actual
            .Should()
            .NotBeNull()
            .And.BeOfType<SystemTextJsonAdapter>();
    }

    [Fact]
    public void Given_that_adapter_is_registered_when_getting_adapter_it_should_return_same_instance()
    {
        using var msg = new HttpRequestMessage();
        IJsonAdapter adapter = Substitute.For<IJsonAdapter>();
        var items = new Dictionary<Type, object> { { typeof(IJsonAdapter), adapter } };
        var sut = new MockHttpRequestContext(msg, items);
        sut.Services.Should().NotBeEmpty();

        // Act
        IJsonAdapter actual = sut.GetAdapter();

        // Assert
        actual
            .Should()
            .NotBeNull()
            .And.BeSameAs(adapter);
    }
}
