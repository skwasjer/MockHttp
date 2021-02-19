using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp
{
	public class InvertRequestMatchingTests : IDisposable
	{
		private readonly MockHttpHandler _mockHttp;
		private readonly HttpClient _httpClient;

		public InvertRequestMatchingTests()
		{
			_mockHttp = new MockHttpHandler();
			_httpClient = new HttpClient(_mockHttp)
			{
				BaseAddress = new Uri("http://0.0.0.1")
			};
		}

		[Theory]
		[InlineData("POST", "GET")]
		[InlineData("POST", "PUT")]
		[InlineData("HEAD", "DELETE")]
		[InlineData("GET", "POST")]
		public async Task Given_request_has_non_matching_method_when_sending_with_negated_setup_it_should_match_mock(string notMethod, string sendMethod)
		{
			_mockHttp
				.When(m => m.Not.Method(notMethod))
				.Respond(HttpStatusCode.OK)
				.Verifiable();

			// Act
			HttpResponseMessage response = await _httpClient.SendAsync(new HttpRequestMessage(new HttpMethod(sendMethod), ""));

			// Assert
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			_mockHttp.Verify();
			_mockHttp.VerifyNoOtherRequests();
		}

		[Theory]
		[InlineData("POST", "POST")]
		[InlineData("DELETE", "DELETE")]
		[InlineData("HEAD", "HEAD")]
		public async Task Given_request_has_matching_method_when_sending_with_negated_setup_it_should_not_match_mock(string notMethod, string sendMethod)
		{
			_mockHttp
				.When(m => m.Not.Method(notMethod))
				.Respond(HttpStatusCode.OK)
				.Verifiable();

			// Act
			HttpResponseMessage response = await _httpClient.SendAsync(new HttpRequestMessage(new HttpMethod(sendMethod), ""));

			// Assert
			response.Should().HaveStatusCode(HttpStatusCode.NotFound);
			Action verify = () => _mockHttp.Verify();
			verify.Should().ThrowExactly<HttpMockException>()
				.WithMessage($"There are 1 unfulfilled expectations:*Not Method: {notMethod}*");
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
			_mockHttp?.Dispose();
		}
	}
}
