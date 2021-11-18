using System.Net;
using System.Text;
using MockHttp.FluentAssertions;
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
            .When(m => m.JsonContent(obj))
            .Respond(HttpStatusCode.OK);

        // Act
        HttpResponseMessage response = await _httpClient.PostAsync("http://0.0.0.0", new StringContent("{\"SomeProperty\": \"value\"}", Encoding.UTF8, "application/json"));

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Given_no_json_content_when_request_content_is_empty_should_be_true(bool hasContent)
    {
        _httpMock
            .When(m => m.JsonContent((object)null))
            .Respond(HttpStatusCode.OK);

        StringContent content = hasContent
            ? new StringContent("", Encoding.UTF8, "application/json")
            : null;

        // Act
        HttpResponseMessage response = await _httpClient.PostAsync("http://0.0.0.0", content);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task When_matching_with_custom_serializerSettings_it_should_match(bool useDefaultSerializer)
    {
        JsonSerializerSettings serializerSettings = useDefaultSerializer
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
            .When(m => m.JsonContent(testClass, serializerSettings))
            .Respond(HttpStatusCode.OK);

        // Act
        HttpResponseMessage response = await _httpClient.PostAsync("http://0.0.0.0", new StringContent(postJson, Encoding.UTF8, "application/json"));

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
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
            _httpMock.UseJsonSerializerSettings(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
        }

        _httpMock
            .When(m => m.JsonContent(testClass))
            .Respond(HttpStatusCode.OK);

        // Act
        HttpResponseMessage response = await _httpClient.PostAsync("http://0.0.0.0", new StringContent(postJson, Encoding.UTF8, "application/json"));

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
    }
}
