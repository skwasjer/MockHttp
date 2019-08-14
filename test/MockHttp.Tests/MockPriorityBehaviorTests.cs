using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Moq;
using Xunit;

namespace MockHttp
{
	public class MockPriorityBehaviorTests
	{
		[Fact]
		public async Task Given_request_is_setup_twice_when_sending_request_last_setup_wins()
		{
			using (var httpMock = new MockHttpHandler())
			using (var httpClient = new HttpClient(httpMock)
			{
				BaseAddress = new Uri("http://0.0.0.0")
			})
			{
				httpMock.When(matching => matching.Method("POST")).Respond(HttpStatusCode.OK);
				httpMock.When(matching => matching.Method("POST")).Respond(HttpStatusCode.Accepted);
				httpMock.When(matching => matching.Method("PUT")).Respond(HttpStatusCode.BadRequest);

				// Act
				var response1 = await httpClient.PostAsync("", new StringContent("data 1"));
				var response2 = await httpClient.PostAsync("", new StringContent("data 2"));
				var response3 = await httpClient.PutAsync("", new StringContent("data 3"));

				// Assert
				response1.Should().HaveStatusCode(HttpStatusCode.Accepted, "the second setup wins on first request");
				response2.Should().HaveStatusCode(HttpStatusCode.Accepted, "the second setup wins on second request");
				response3.Should().HaveStatusCode(HttpStatusCode.BadRequest, "the request was sent with different HTTP method matching third setup");
			}
		}

		/// <summary>
		/// Asserting behavior of Moq that last setup wins, unless parameters are different.
		/// We want the same behavior.
		/// </summary>
		[Fact]
		public void Given_moq_is_setup_twice_when_calling_mocked_method_last_setup_wins()
		{
			var mock = new Mock<ITest>();
			mock.Setup(m => m.GetResult(1)).Returns(1);
			mock.Setup(m => m.GetResult(1)).Returns(2);
			mock.Setup(m => m.GetResult(It.IsAny<int>())).Returns(3);
			mock.Setup(m => m.GetResult(2)).Returns(4);

			// Act
			mock.Object.GetResult(1).Should().Be(3, "the third setup wins on first call");
			mock.Object.GetResult(1).Should().Be(3, "the third setup wins on second call");

			// But of course, different params do give different result.
			mock.Object.GetResult(2).Should().Be(4, "the method was called with different param matching fourth setup");
		}

		public interface ITest
		{
			object GetResult(int param);
		}
	}
}
