using System.Net;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using MockHttp.FluentAssertions;
using MockHttp.Language;
using MockHttp.Language.Flow;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace MockHttp.Json;

public sealed class JsonRespondsExtensionsTests : IDisposable
{
    private readonly IResponds<ISequenceResponseResult> _sut;
    private readonly MockHttpHandler _httpMock;

    private readonly HttpClient _httpClient;

    public JsonRespondsExtensionsTests()
    {
        _httpMock = new MockHttpHandler();
        _httpClient = new HttpClient(_httpMock) { BaseAddress = new Uri("http://0.0.0.0") };
        _sut = _httpMock.When(_ => { });
    }

    public void Dispose()
    {
        _httpMock?.Dispose();
        _httpClient?.Dispose();
    }

    [Fact]
    public async Task When_responding_with_json_object_it_should_return_response()
    {
        var jsonContent = new { name = "John Doe" };
        var request = new HttpRequestMessage();

        // Act
        _sut.RespondJson(jsonContent);
        HttpResponseMessage actualResponse = await _httpClient.SendAsync(request, CancellationToken.None);

        // Assert
        await actualResponse.Should()
            .HaveStatusCode(HttpStatusCode.OK)
            .And.HaveJsonContent(jsonContent);
    }

    [Theory]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task When_responding_with_statusCode_and_json_object_it_should_return_response(HttpStatusCode statusCode)
    {
        var jsonContent = new { name = "John Doe" };
        var request = new HttpRequestMessage();

        // Act
        _sut.RespondJson(statusCode, jsonContent);
        HttpResponseMessage actualResponse = await _httpClient.SendAsync(request, CancellationToken.None);

        // Assert
        await actualResponse.Should()
            .HaveStatusCode(statusCode)
            .And.HaveJsonContent(jsonContent);
    }

    [Fact]
    public async Task When_responding_without_setting_media_type_it_should_return_with_correct_content_type_header()
    {
        const string expectedDefaultContentType = "application/json; charset=utf-8";
        var request = new HttpRequestMessage();

        // Act
        _sut.RespondJson(new object());
        HttpResponseMessage actualResponse = await _httpClient.SendAsync(request, CancellationToken.None);

        // Assert
        actualResponse.Should().HaveContentType(expectedDefaultContentType);
    }

    [Fact]
    public async Task When_responding_with_custom_media_type_it_should_return_with_correct_content_type_header()
    {
        const string contentType = "application/problem+json";
        var request = new HttpRequestMessage();

        // Act
        _sut.RespondJson(new object(), contentType);
        HttpResponseMessage actualResponse = await _httpClient.SendAsync(request, CancellationToken.None);

        // Assert
        actualResponse.Should().HaveContentType(contentType);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task When_responding_with_custom_serializerSettings_it_should_return_correct_json(bool useDefaultSerializer)
    {
        var request = new HttpRequestMessage();
        JsonSerializerSettings serializerSettings = useDefaultSerializer 
            ? null 
            : new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        string expectedJson = useDefaultSerializer ? "{\"SomeProperty\":\"value\"}" : "{\"some_property\":\"value\"}";
        var testClass = new TestClass { SomeProperty = "value" };

        // Act
        _sut.RespondJson(testClass, null, serializerSettings);
        HttpResponseMessage actualResponse = await _httpClient.SendAsync(request, CancellationToken.None);

        // Assert
        await actualResponse.Should().HaveContentAsync(expectedJson);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task When_responding_with_global_serializerSettings_it_should_return_correct_json(bool useDefaultSerializer)
    {
        var request = new HttpRequestMessage();
        string expectedJson = useDefaultSerializer ? "{\"SomeProperty\":\"value\"}" : "{\"some_property\":\"value\"}";
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

        // Act
        _sut.RespondJson(testClass);
        HttpResponseMessage actualResponse = await _httpClient.SendAsync(request, CancellationToken.None);

        // Assert
        await actualResponse.Should().HaveContentAsync(expectedJson);
    }

    [DataContract(Name = "RootElem", Namespace = "")]
    public class MyXmlSerializableType
    {
        [DataMember(Name = "Name")]
        public string Name { get; set; }
    }

    [Fact]
    public async Task When_responding_with_custom_media_type_formatter_it_should_return_content_formatted_with_formatter()
    {
        var responseContent = new MyXmlSerializableType { Name = "John Doe" };
        var request = new HttpRequestMessage();
        var xmlMediaTypeFormatter = new XmlMediaTypeFormatter();

        // Act
        _sut.RespondObject(responseContent, xmlMediaTypeFormatter);
        HttpResponseMessage actualResponse = await _httpClient.SendAsync(request, CancellationToken.None);

        // Assert
        await actualResponse.Should().HaveContentAsync("<RootElem xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><Name>John Doe</Name></RootElem>");
    }

    [Fact]
    public async Task When_responding_with_custom_media_type_formatter_and_media_type_it_should_return_with_correct_content_type_header()
    {
        const string contentType = "fake/xml+type";
        var request = new HttpRequestMessage();
        var xmlMediaTypeFormatter = new XmlMediaTypeFormatter();

        // Act
        _sut.RespondObject(new MyXmlSerializableType(), xmlMediaTypeFormatter, contentType);
        HttpResponseMessage actualResponse = await _httpClient.SendAsync(request, CancellationToken.None);

        // Assert
        actualResponse.Should().HaveContentType(contentType);
    }
}
