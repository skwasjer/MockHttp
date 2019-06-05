using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using HttpClientMock.HttpRequestMatchers;
using Moq;
using Xunit;

namespace HttpClientMock.Tests
{
	public class HttpClientMockHandlerTests : IDisposable
	{
		private readonly HttpClientMockHandler _sut;
		private readonly HttpClient _httpClient;

		public HttpClientMockHandlerTests()
		{
			_sut = new HttpClientMockHandler();
			_httpClient = new HttpClient(_sut);
		}

		public void Dispose()
		{
			_sut?.Dispose();
			_httpClient?.Dispose();
		}

		[Fact]
		public async Task Test()
		{
			MockedHttpRequest mockedRequest = _sut
				.WhenRequesting("http://0.0.0.0/")
				.With(new HttpMethodMatcher("GET"))
				.RespondsWith(message => new HttpResponseMessage(HttpStatusCode.Accepted)) as MockedHttpRequest;

			mockedRequest.Callback(() =>
			{

			});

			var response = await _httpClient.GetAsync("http://0.0.0.0/");

			_sut.Verify(mockedRequest, 1, "a request was posted");

			response.StatusCode.Should().Be(HttpStatusCode.Accepted);
		}
	}
}
