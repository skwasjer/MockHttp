using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace HttpClientMock
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

		/// <summary>
		/// Exception that we can test <see cref="IThrows"/> logic with.
		/// </summary>
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
				.RespondWith(response);

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
				.RespondWith(() => new HttpResponseMessage
				{
					ReasonPhrase = $"Callback is called = {callbackCalled}"
				});

			// Act
			HttpResponseMessage actualResponse = await _httpClient.GetAsync("");

			// Assert
			actualResponse.ReasonPhrase.Should().Be(expectedReason, "the callback should be called before we returned the response");
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
				.Throw<HttpMockException>("the mock is configured without a response")
				.WithMessage("No response configured for mock.");
		}

		[Fact]
		public async Task Given_verification_by_condition_fails_when_verifying_should_throw()
		{
			await _httpClient.GetAsync("");

			// Act
			Action verifyPost = () => _sut.Verify(
				// Matching by POST should cause verification to throw.
				matching => matching.Method(HttpMethod.Post),
				IsSent.Once
			);

			// Assert
			verifyPost.Should()
				.Throw<HttpMockException>("a GET request was sent instead of POST")
				.WithMessage("Expected request to have been sent exactly once* 0 time*");
		}

		[Fact]
		public async Task Given_verification_by_condition_succeeds_when_verifying_should_not_throw()
		{
			await _httpClient.GetAsync("");

			// Act
			Action verifyGet = () => _sut.Verify(
				matching => matching.Method(HttpMethod.Get),
				IsSent.Once
			);

			// Assert
			verifyGet.Should().NotThrow<HttpMockException>();
		}

		[Fact]
		public async Task Given_expectation_fails_when_verifying_should_throw()
		{
			_sut.When(matching => matching.Method(HttpMethod.Post))
				.RespondWith(HttpStatusCode.OK)
				.Verifiable();

			await _httpClient.GetAsync("");

			// Act
			Action verifyPost = () => _sut.Verify();

			// Assert
			verifyPost.Should()
				.Throw<HttpMockException>("a GET request was sent instead of POST")
				.WithMessage("There are 1 unfulfilled expectations*");
		}

		[Fact]
		public async Task Given_expectation_succeeds_when_verifying_should_throw()
		{
			_sut.When(matching => matching.Method(HttpMethod.Get))
				.RespondWith(HttpStatusCode.OK)
				.Verifiable();

			await _httpClient.GetAsync("");

			// Act
			Action verifyGet = () => _sut.Verify();

			// Assert
			verifyGet.Should().NotThrow<HttpMockException>();
		}

		[Theory]
		[MemberData(nameof(IsSentVerifiesSuccessfullyTestCases))]
		public async Task Given_number_of_requests_match_expected_callCount_when_verifying_should_not_throw(int requestCount, IsSent isSent)
		{
			for (int i = 0; i < requestCount; i++)
			{
				await _httpClient.GetAsync("url");
			}

			// Act
			Action verifyGet = () => _sut.Verify(
				matching => matching.Url("**/url"),
				isSent
			);

			// Assert
			verifyGet.Should().NotThrow<HttpMockException>();
		}

		// ReSharper disable once MemberCanBePrivate.Global
		public static IEnumerable<object[]> IsSentVerifiesSuccessfullyTestCases
		{
			get
			{
				yield return new object[] { 1, IsSent.AtLeast(1) };
				yield return new object[] { 3, IsSent.AtLeast(3) };
				yield return new object[] { 5, IsSent.AtLeast(3) };
				yield return new object[] { 1, IsSent.AtLeastOnce() };
				yield return new object[] { 3, IsSent.AtLeastOnce() };
				yield return new object[] { 1, IsSent.AtMost(1) };
				yield return new object[] { 3, IsSent.AtMost(3) };
				yield return new object[] { 3, IsSent.AtMost(5) };
				yield return new object[] { 0, IsSent.AtMostOnce() };
				yield return new object[] { 1, IsSent.AtMostOnce() };
				yield return new object[] { 1, IsSent.Exactly(1) };
				yield return new object[] { 3, IsSent.Exactly(3) };
				yield return new object[] { 0, IsSent.Never() };
				yield return new object[] { 1, IsSent.Once() };
			}
		}

		[Theory]
		[MemberData(nameof(IsSentFailsVerificationTestCases))]
		public async Task Given_number_of_requests_does_not_match_expected_callCount_when_verifying_should_throw(int requestCount, IsSent isSent)
		{
			for (int i = 0; i < requestCount; i++)
			{
				await _httpClient.GetAsync("url");
			}

			// Act
			Action verifyGet = () => _sut.Verify(
				matching => matching.Url("**/url"),
				isSent
			);

			// Assert
			verifyGet.Should().Throw<HttpMockException>()
				.WithMessage("Expected request to have*");
		}

		// ReSharper disable once MemberCanBePrivate.Global
		public static IEnumerable<object[]> IsSentFailsVerificationTestCases
		{
			get
			{
				yield return new object[] { 0, IsSent.AtLeast(1) };
				yield return new object[] { 1, IsSent.AtLeast(3) };
				yield return new object[] { 2, IsSent.AtLeast(3) };
				yield return new object[] { 0, IsSent.AtLeastOnce() };
				yield return new object[] { 2, IsSent.AtMost(1) };
				yield return new object[] { 4, IsSent.AtMost(2) };
				yield return new object[] { 2, IsSent.AtMostOnce() };
				yield return new object[] { 4, IsSent.AtMostOnce() };
				yield return new object[] { 0, IsSent.Exactly(1) };
				yield return new object[] { 3, IsSent.Exactly(2) };
				yield return new object[] { 1, IsSent.Never() };
				yield return new object[] { 3, IsSent.Never() };
				yield return new object[] { 0, IsSent.Once() };
				yield return new object[] { 2, IsSent.Once() };
				yield return new object[] { 3, IsSent.Once() };
			}
		}

		[Fact]
		public async Task Test()
		{
			_sut
				.When(matching => matching
					.Url("http://0.0.0.1/**")
					.QueryString("test", "$%^&*")
					.QueryString("test2", "value")
					.Method("GET")
					.Content("")
				)
				.Callback(() =>
				{
				})
				//.Throws<Exception>();
				.RespondWith(() =>
					new HttpResponseMessage(HttpStatusCode.Accepted)
				)
				.Verifiable();

			var response = await _httpClient.GetAsync("http://0.0.0.1/controller/action?test=1");
			response = await _httpClient.GetAsync("http://0.0.0.1/controller/action?test=%24%25^%26*&test2=value");
			

			_sut.Verify(matching => matching.Url("**/controller/**"), IsSent.Exactly(2), "we sent it");
			_sut.Verify();
			_sut.VerifyAll();

			response.StatusCode.Should().Be(HttpStatusCode.Accepted);
		}

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
