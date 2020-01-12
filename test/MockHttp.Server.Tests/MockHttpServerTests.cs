using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Server
{
	public class MockHttpServerTests : IClassFixture<MockHttpServerFixture>
	{
		private readonly MockHttpServerFixture _fixture;

		public MockHttpServerTests(MockHttpServerFixture fixture)
		{
			_fixture = fixture;
			_fixture.MockHttp.Reset();
		}

		[Fact]
		public async Task Given_mocked_server_when_sending_client_request_it_should_respond()
		{
			using HttpClient client = _fixture.Server.CreateClient();

			_fixture.MockHttp
				.When(matching => matching
					.RequestUri("test/wtf/")
					.Header("test", "value")
				)
				.Respond(HttpStatusCode.OK, "Some content", "text/html");

			// Act
			using var request = new HttpRequestMessage(HttpMethod.Get, "test/wtf/")
			{
				Headers =
				{
					{ "test", "value"}
				}
			};

			HttpResponseMessage response = await client.SendAsync(request);

			// Assert
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			await response.Should().HaveContentAsync("Some content");
		}
	}
}
