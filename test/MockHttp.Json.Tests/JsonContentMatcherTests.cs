using MockHttp.Responses;

namespace MockHttp.Json;

public sealed class JsonContentMatcherTests : IDisposable
{
    private readonly Mock<IJsonAdapter> _adapterMock;
    private readonly Mock<IEqualityComparer<string>> _equalityComparerMock;
    private readonly Dictionary<Type, object> _services;
    private readonly HttpRequestMessage _requestMessage;
    private readonly MockHttpRequestContext _requestContext;

    public JsonContentMatcherTests()
    {
        _adapterMock = new Mock<IJsonAdapter>();

        _equalityComparerMock = new Mock<IEqualityComparer<string>>();
        _equalityComparerMock
            .Setup(m => m.Equals(It.IsAny<string?>()!, It.IsAny<string?>()!))
            .Returns(true);

        _services = new Dictionary<Type, object>();
        _requestMessage = new HttpRequestMessage();
        _requestContext = new MockHttpRequestContext(_requestMessage, _services);
    }

    [Fact]
    public async Task Given_that_adapter_is_provided_to_ctor_when_matching_it_should_use_the_adapter()
    {
        var jsonContentAsObject = new { PropertyName = "value" };
        const string serializedJson = "{\"modifiedPropertyName\":\"modifiedValue\"}";
        _adapterMock
            .Setup(m => m.Serialize(jsonContentAsObject))
            .Returns(serializedJson)
            .Verifiable();

        var globalAdapterMock = new Mock<IJsonAdapter>();
        _services.Add(typeof(IJsonAdapter), globalAdapterMock.Object);

        var sut = new JsonContentMatcher(jsonContentAsObject, _adapterMock.Object, _equalityComparerMock.Object);

        // Act
        await sut.IsMatchAsync(_requestContext);

        // Assert
        _adapterMock.Verify();
        globalAdapterMock.Verify(m => m.Serialize(It.IsAny<object?>()), Times.Never);
        _equalityComparerMock
            .Verify(m => m.Equals(It.IsAny<string>(), serializedJson),
                Times.Once
            );
    }

    [Fact]
    public async Task Given_that_adapter_is_not_provided_to_ctor_when_matching_it_should_use_the_global_adapter()
    {
        var jsonContentAsObject = new { PropertyName = "value" };
        const string serializedJson = "{\"modifiedPropertyName\":\"modifiedValue\"}";

        var globalAdapterMock = new Mock<IJsonAdapter>();
        globalAdapterMock
            .Setup(m => m.Serialize(jsonContentAsObject))
            .Returns(serializedJson)
            .Verifiable();

        var sut = new JsonContentMatcher(jsonContentAsObject, null, _equalityComparerMock.Object);

        // Act
        _services.Add(typeof(IJsonAdapter), globalAdapterMock.Object);
        await sut.IsMatchAsync(_requestContext);

        // Assert
        globalAdapterMock.Verify();
        _equalityComparerMock
            .Verify(m => m.Equals(It.IsAny<string>(), serializedJson),
                Times.Once
            );
    }

    [Fact]
    public async Task Given_that_adapter_is_not_provided_to_ctor_and_no_global_adapter_is_available_when_matching_it_should_use_the_default()
    {
        var jsonContentAsObject = new { PropertyName = "value" };
        const string serializedJson = "{\"PropertyName\":\"value\"}";
        var sut = new JsonContentMatcher(jsonContentAsObject, null, _equalityComparerMock.Object);

        // Act
        _services.Should().NotContainKey(typeof(IJsonAdapter));
        await sut.IsMatchAsync(_requestContext);

        // Assert
        _equalityComparerMock
            .Verify(m => m.Equals(It.IsAny<string>(), serializedJson),
                Times.Once
            );
    }

    [Fact]
    public async Task Given_that_adapter_is_not_provided_to_ctor_when_matching_it_should_use_the_adapter()
    {
        const string jsonContentAsObject = "text";
        var sut = new JsonContentMatcher(jsonContentAsObject, _adapterMock.Object, _equalityComparerMock.Object);

        // Act
        await sut.IsMatchAsync(_requestContext);

        // Assert
        _adapterMock.Verify(m => m.Serialize(jsonContentAsObject), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task When_matching_it_should_return_the_results_of_the_comparer(bool equals)
    {
        var sut = new JsonContentMatcher("something to compare with", _adapterMock.Object, _equalityComparerMock.Object);

        _equalityComparerMock
            .Setup(m => m.Equals(It.IsAny<string?>()!, It.IsAny<string?>()!))
            .Returns(equals)
            .Verifiable();

        // Act
        bool actual = await sut.IsMatchAsync(_requestContext);

        // Assert
        _equalityComparerMock.Verify();
        actual.Should().Be(equals);
    }

    [Theory]
    [MemberData(nameof(EmptyContentTestCases))]
    public async Task Given_that_request_content_is_null_or_empty_when_matching_it_should_compare_with_empty_string
    (
        HttpContent content
    )
    {
        using (content)
        {
            var sut = new JsonContentMatcher("something to compare with", _adapterMock.Object, _equalityComparerMock.Object);
            _requestMessage.Content = content;

            // Act
            await sut.IsMatchAsync(_requestContext);

            // Assert
            _equalityComparerMock
                .Verify(m => m.Equals(string.Empty, It.IsAny<string>()),
                    Times.Once
                );
        }
    }

    public static IEnumerable<object?[]> EmptyContentTestCases()
    {
        yield return new object?[] { null };
        // Edge case (empty content, but with content length > 0)
        yield return new object?[] { new StringContent(string.Empty) { Headers = { ContentLength = 10 } } };
        // Edge case (not empty content, but with content length == 0)
        yield return new object?[] { new StringContent("not empty") { Headers = { ContentLength = 0 } } };
    }

    [Theory]
    [MemberData(nameof(JsonMatchTestCases))]
    public async Task Given_source_json_when_matching_it_should_succeed(object expectedJsonContentAsObject, string requestJson)
    {
        var request = new HttpRequestMessage { Content = new StringContent(requestJson) };
        var context = new MockHttpRequestContext(request);
        var sut = new JsonContentMatcher(expectedJsonContentAsObject);

        // Act
        bool result = await sut.IsMatchAsync(context);

        // Assert
        result.Should().BeTrue();
    }

    public static IEnumerable<object[]> JsonMatchTestCases()
    {
        yield return new object[] { new[] { 1, 2, 3 }, "[1,2,3]" };
        yield return new object[] { new { value = 1 }, "{\"value\":1}" };
        yield return new object[] { "some text", "\"some text\"" };
        yield return new object[] { 123, "123" };
    }

    public void Dispose()
    {
        _requestMessage?.Dispose();
    }
}
