using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using HttpClientMock.Language;
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

		public class TestClass
		{
			public virtual int Test()
			{
				return 0;
			}
		}

		[Fact]
		public async Task Test()
		{
			IResponseResult mockedRequest = _sut
				.When(matching => matching
					.Url("http://0.0.0.1/**")
					.Method("GET")
					.Content("")
				)
				.Callback(() =>
				{
				})
				//.Throws<Exception>();
				.RespondWithAsync(() => 
					new HttpResponseMessage(HttpStatusCode.Accepted));

			mockedRequest.Verifiable();

			//			_sut.Add(mockedRequest);

			var response = await _httpClient.GetAsync("http://0.0.0.1/controller/action?test=1");

			_sut.Verify(matching => matching.Url("**/controller/**"));
			//_sut.Verify((IMockedHttpRequest)mockedRequest, 1, "a request was posted");
			_sut.Verify();

			response.StatusCode.Should().Be(HttpStatusCode.Accepted);
		}

		[Fact]
		public void T()
		{
			Mock<TestClass> mock = new Mock<TestClass>();
			int i = 0;
			mock
				.Setup(c => c.Test())
				.Callback(
					() => 
						i += 1)
				.Returns(
					() =>
						i
					);
//			mock.Invocations.Any(i => i.)
			var result = mock.Object.Test();

			result.Should().Be(1);

			//			mock.Verify();
		}
	}
}
