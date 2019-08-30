using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
#if !NETCOREAPP1_1
using System.Net.Http.Formatting;
#endif
using System.Threading;
using System.Threading.Tasks;
using MockHttp.FluentAssertions;
using MockHttp.Language;
using MockHttp.Language.Flow;
using Xunit;

namespace MockHttp.Json
{
	public class JsonRespondsExtensionsTests : IDisposable
	{
		private readonly IResponds<ISequenceResponseResult> _sut;
		private readonly MockHttpHandler _httpMock;

		private readonly HttpClient _httpClient;

		public JsonRespondsExtensionsTests()
		{
			_httpMock = new MockHttpHandler();
			_httpClient = new HttpClient(_httpMock)
			{
				BaseAddress = new Uri("http://0.0.0.0")
			};
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
			var jsonContent = new
			{
				name = "John Doe"
			};
			var request = new HttpRequestMessage();

			// Act
			_sut.RespondJson(jsonContent);
			HttpResponseMessage actualResponse = await _httpClient.SendAsync(request, CancellationToken.None);

			// Assert
			actualResponse.Should()
				.HaveStatusCode(HttpStatusCode.OK)
				.And.HaveJsonContent(jsonContent);
		}

		[Theory]
		[InlineData(HttpStatusCode.Accepted)]
		[InlineData(HttpStatusCode.BadRequest)]
		public async Task When_responding_with_statusCode_and_json_object_it_should_return_response(HttpStatusCode statusCode)
		{
			var jsonContent = new
			{
				name = "John Doe"
			};
			var request = new HttpRequestMessage();

			// Act
			_sut.RespondJson(statusCode, jsonContent);
			HttpResponseMessage actualResponse = await _httpClient.SendAsync(request, CancellationToken.None);

			// Assert
			actualResponse.Should()
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

#if !NETCOREAPP1_1

		[DataContract(Name = "RootElem", Namespace = "")]
		public class MyXmlSerializableType
		{
			[DataMember(Name = "Name")]
			public string Name { get; set; }
		}

		[Fact]
		public async Task When_responding_with_custom_media_type_formatter_it_should_return_content_formatted_with_formatter()
		{
			var responseContent = new MyXmlSerializableType
			{
				Name = "John Doe"
			};
			var request = new HttpRequestMessage();
			var xmlMediaTypeFormatter = new XmlMediaTypeFormatter();

			// Act
			_sut.RespondObject(responseContent, xmlMediaTypeFormatter);
			HttpResponseMessage actualResponse = await _httpClient.SendAsync(request, CancellationToken.None);

			// Assert
			actualResponse.Should().HaveContentAsync("<RootElem xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><Name>John Doe</Name></RootElem>");
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
#endif
	}
}

