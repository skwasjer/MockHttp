using System;
using System.Net;
using System.Net.Http;
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
	}
}

