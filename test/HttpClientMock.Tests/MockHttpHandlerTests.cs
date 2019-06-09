using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using HttpClientMock.Language.Flow;
using Moq;
using Xunit;

namespace HttpClientMock.Tests
{
	public class MockHttpHandlerTests : IDisposable
	{
		private readonly MockHttpHandler _sut;
		private readonly HttpClient _httpClient;

		public MockHttpHandlerTests()
		{
			_sut = new MockHttpHandler();
			_httpClient = new HttpClient(_sut)
			{
				BaseAddress = new Uri("http://0.0.0.0")
			};
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

		private class TestableException : Exception
		{
		}

		[Fact]
		public void Given_request_is_configured_to_throw_when_sending_request_should_throw_exception()
		{
			_sut.When(matching => { })
				.Throws<TestableException>();

			// Act
			Func<Task<HttpResponseMessage>> act = () => _httpClient.GetAsync("");

			// Assert
			act.Should().ThrowExactly<TestableException>();
			_sut.Verify(m => { }, IsSent.Once);
		}

		[Fact]
		public void Given_request_is_configured_to_throw_specific_exception_when_sending_request_should_throw_exception()
		{
			var exception = new TestableException();
			_sut.When(matching => { })
				.Throws(exception);

			// Act
			Func<Task<HttpResponseMessage>> act = () => _httpClient.GetAsync("");

			// Assert
			act.Should().Throw<TestableException>()
				.Which.Should().Be(exception);
			_sut.Verify(m => { }, IsSent.Once);
		}

		[Fact]
		public async Task Given_request_is_configured_to_return_response_when_sending_request_should_return_response()
		{
			var response = new HttpResponseMessage();
			_sut.When(matching => { })
				.RespondWithAsync(() => response);

			// Act
			HttpResponseMessage actualResponse = await _httpClient.GetAsync("");

			// Assert
			actualResponse.Should().BeSameAs(response);
			_sut.Verify(m => { }, IsSent.Once);
		}

		[Fact]
		public async Task Given_no_request_is_configured_when_sending_request_should_return_default_response()
		{
			// Act
			HttpResponseMessage actualResponse = await _httpClient.GetAsync("");

			// Assert
			actualResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
			actualResponse.ReasonPhrase.Should().Be("No request is configured, returning default response.");
			_sut.Verify(m => { }, IsSent.Once);
		}

		[Fact]
		public async Task Given_a_callback_is_configured_when_sending_request_should_invoke_callback_before_sending_request()
		{
			 bool callbackCalled = false;
			 const string expectedReason = "Callback is called = True";
			 _sut.When(matching => { })
				 .Callback(() => callbackCalled = true)
				 .RespondWithAsync(() => new HttpResponseMessage
				 {
					 ReasonPhrase = $"Callback is called = {callbackCalled}"
				 });

			// Act
			HttpResponseMessage actualResponse = await _httpClient.GetAsync("");

			// Assert
			actualResponse.ReasonPhrase.Should().Be(expectedReason, "the callback should be called before we built the response");
			_sut.Verify(m => { }, IsSent.Once);
		}

		[Fact]
		public void Given_no_response_is_configured_when_sending_request_should_throw()
		{
			_sut.When(matching => { });

			// Act
			Func<Task<HttpResponseMessage>> act = () => _httpClient.GetAsync("");

			// Assert
			act.Should()
				.Throw<HttpMockException>()
				.WithMessage("No response configured for mock.");
		}

		//[Fact]
		//public async Task Test()
		//{
		//	_sut
		//		.When(matching => matching
		//			.Url("http://0.0.0.1/**")
		//			.Method("GET")
		//			.Content("")
		//		)
		//		.Callback(() =>
		//		{
		//		})
		//		//.Throws<Exception>();
		//		.RespondWithAsync(() => 
		//			new HttpResponseMessage(HttpStatusCode.Accepted)
		//		)
		//		.Verifiable();

		//	var response = await _httpClient.GetAsync("http://0.0.0.1/controller/action?test=1");
		//	response = await _httpClient.GetAsync("http://0.0.0.1/controller/action?test=1");

		//	_sut.Verify(matching => matching.Url("**/controller/**"), IsSent.Exactly(2), "we sent it");
		//	_sut.Verify();
		//	_sut.VerifyAll();

		//	response.StatusCode.Should().Be(HttpStatusCode.Accepted);
		//}

//		[Fact]
//		public void T()
//		{
//			Mock<TestClass> mock = new Mock<TestClass>();
//			//mock.Verify(_ => {}, new Times());
//			mock.Reset();
//			int i = 0;
//			mock
//				.Setup(c => c.Test())
//				.Callback(
//					() => 
//						i += 1)
//				.Returns(
//					() =>
//						i
//					);
////			mock.Invocations.Any(i => i.)
//			var result = mock.Object.Test();

//			result.Should().Be(1);

//			//			mock.Verify();
//		}
	}
}
