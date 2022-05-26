using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using MockHttp.Http;
using MockHttp.Json.Newtonsoft;
using MockHttp.Json.SystemTextJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace MockHttp.Json;

public class JsonRequestMatchingExtensionTests
{
    private readonly MockHttpHandler _httpMock;
    private readonly HttpClient _httpClient;

    public JsonRequestMatchingExtensionTests()
    {
        _httpMock = new MockHttpHandler();
        _httpClient = new HttpClient(_httpMock) { BaseAddress = new Uri("http://0.0.0.0") };
    }

    [Fact]
    public async Task Given_json_content_matching_request_when_matching_should_be_true()
    {
        var obj = new TestClass { SomeProperty = "value" };

        _httpMock
            .When(m => m.JsonBody(obj))
            .Respond(with => with.StatusCode(HttpStatusCode.OK))
            .Verifiable();

        // Act
        HttpResponseMessage response = await _httpClient.PostAsync("http://0.0.0.0", new StringContent("{\"SomeProperty\": \"value\"}", Encoding.UTF8, MediaTypes.Json));

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        _httpMock.Verify();
    }

    [Fact]
    public async Task Given_not_a_json_content_matching_request_and_a_different_request_uri_when_matching_should_not_test_others_matcher()
    {
        var obj = new TestClass { SomeProperty = "value" };

        _httpMock
            .When(m => m
                .RequestUri("/users")
                .JsonBody(obj))
            .Respond(with => with.StatusCode(HttpStatusCode.OK));

        // Act
        Func<Task> act = () => _httpClient.PostAsync("http://0.0.0.0", new StringContent("test=test", Encoding.UTF8, MediaTypes.FormUrlEncoded));

        // Assert
        await act.Should().NotThrowAsync<System.Text.Json.JsonException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Given_no_json_content_when_request_content_is_empty_should_be_true(bool hasContent)
    {
        _httpMock
            .When(m => m.JsonBody((object?)null))
            .Respond(with => with.StatusCode(HttpStatusCode.OK))
            .Verifiable();

        StringContent? content = hasContent
            ? new StringContent("", Encoding.UTF8, MediaTypes.Json)
            : null;

        // Act
        HttpResponseMessage response = await _httpClient.PostAsync("http://0.0.0.0", content!);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        _httpMock.Verify();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task When_matching_with_custom_serializerSettings_it_should_match(bool useDefaultSerializer)
    {
        JsonSerializerSettings? serializerSettings = useDefaultSerializer
            ? null
            : new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        string postJson = useDefaultSerializer ? "{\"SomeProperty\":\"value\"}" : "{\"some_property\":\"value\"}";
        var testClass = new TestClass { SomeProperty = "value" };

        _httpMock
            .When(m => m.JsonBody(testClass, serializerSettings))
            .Respond(with => with.StatusCode(HttpStatusCode.OK))
            .Verifiable();

        // Act
        HttpResponseMessage response = await _httpClient.PostAsync("http://0.0.0.0", new StringContent(postJson, Encoding.UTF8, MediaTypes.Json));

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        _httpMock.Verify();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task When_matching_with_custom_serializerOptions_it_should_match(bool useDefaultSerializer)
    {
        JsonSerializerOptions? serializerOptions = useDefaultSerializer
            ? null
            : new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        string postJson = useDefaultSerializer ? "{\"SomeProperty\":\"value\"}" : "{\"someProperty\":\"value\"}";
        var testClass = new TestClass { SomeProperty = "value" };

        _httpMock
            .When(m => m.JsonBody(testClass, serializerOptions))
            .Respond(with => with.StatusCode(HttpStatusCode.OK))
            .Verifiable();

        // Act
        HttpResponseMessage response = await _httpClient.PostAsync("http://0.0.0.0", new StringContent(postJson, Encoding.UTF8, MediaTypes.Json));

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        _httpMock.Verify();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task When_matching_with_global_serializerSettings_it_should_match(bool useDefaultSerializer)
    {
        string postJson = useDefaultSerializer ? "{\"SomeProperty\":\"value\"}" : "{\"some_property\":\"value\"}";
        var testClass = new TestClass { SomeProperty = "value" };

        if (!useDefaultSerializer)
        {
            _httpMock.UseNewtonsoftJson(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
        }

        _httpMock
            .When(m => m.JsonBody(testClass))
            .Respond(with => with.StatusCode(HttpStatusCode.OK))
            .Verifiable();

        // Act
        HttpResponseMessage response = await _httpClient.PostAsync("http://0.0.0.0", new StringContent(postJson, Encoding.UTF8, MediaTypes.Json));

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        _httpMock.Verify();
    }
}
