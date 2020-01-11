using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Language;
using MockHttp.Language.Flow;
using MockHttp.Responses;
using Moq;
using Xunit;

namespace MockHttp.Extensions
{
	public class IRespondsExtensionsTests
	{
		private readonly IResponds<ISequenceResponseResult> _sut;
		private readonly HttpCall _httpCall;

		public IRespondsExtensionsTests()
		{
			_httpCall = new HttpCall();
			_sut = new HttpRequestSetupPhrase(_httpCall);
		}

		public class WithMessage : IRespondsExtensionsTests
		{
			[Fact]
			public async Task When_responding_with_message_it_should_return_response()
			{
				var response = new HttpResponseMessage();

				// Act
				_sut.Respond(() => response);
				HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(new HttpRequestMessage()), CancellationToken.None);

				// Assert
				actualResponse.Should().BeSameAs(response);
			}

			[Fact]
			public async Task When_responding_with_message_based_on_request_it_should_return_response()
			{
				var request = new HttpRequestMessage(HttpMethod.Get, "http://0.0.0.0/controller/action?query=1");

				// Act
				_sut.Respond(r => new HttpResponseMessage { Headers = { { "query", r.RequestUri.Query } } });
				HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

				// Assert
				actualResponse.Should().HaveHeader("query", request.RequestUri.Query);
			}
		}
		public class WithStream : IRespondsExtensionsTests
		{
			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public async Task When_responding_with_stream_it_should_return_response(bool isSeekable)
			{
				using (var ms = new CanSeekMemoryStream(Encoding.UTF8.GetBytes("content"), isSeekable))
				{
					var request = new HttpRequestMessage();
					var expectedContent = new ByteArrayContent(Encoding.UTF8.GetBytes("content"));

					// Act
					_sut.Respond(ms);
					HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

					// Assert
					await actualResponse.Should()
						.HaveStatusCode(HttpStatusCode.OK)
						.And.HaveContentAsync(expectedContent);

					// Second assert, to test we can read stream twice, even if not seekable.
					await actualResponse.Should()
						.HaveStatusCode(HttpStatusCode.OK)
						.And.HaveContentAsync(expectedContent);
				}
			}

			[Theory]
			[InlineData(HttpStatusCode.Accepted, true)]
			[InlineData(HttpStatusCode.BadGateway, false)]
			public async Task When_responding_with_status_code_stream_it_should_return_response(HttpStatusCode statusCode,
				bool isSeekable)
			{
				using (var ms = new CanSeekMemoryStream(Encoding.UTF8.GetBytes("content"), isSeekable))
				{
					var request = new HttpRequestMessage();
					var expectedContent = new ByteArrayContent(Encoding.UTF8.GetBytes("content"));

					// Act
					_sut.Respond(statusCode, ms);
					HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

					// Assert
					await actualResponse.Should()
						.HaveStatusCode(statusCode)
						.And.HaveContentAsync(expectedContent);

					// Second assert, to test we can read stream twice, even if not seekable.
					await actualResponse.Should()
						.HaveStatusCode(statusCode)
						.And.HaveContentAsync(expectedContent);
				}
			}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public async Task When_responding_with_stream_and_media_type_it_should_return_response(bool isSeekable)
			{
				using (var ms = new CanSeekMemoryStream(Encoding.UTF8.GetBytes("content"), isSeekable))
				{
					var request = new HttpRequestMessage();
					var expectedContent = new StringContent("content", Encoding.UTF8, "text/html");

					// Act
					_sut.Respond(ms, "text/html; charset=utf-8");
					HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

					// Assert
					await actualResponse.Should()
						.HaveStatusCode(HttpStatusCode.OK)
						.And.HaveContentAsync(expectedContent);

					// Second assert, to test we can read stream twice, even if not seekable.
					await actualResponse.Should()
						.HaveStatusCode(HttpStatusCode.OK)
						.And.HaveContentAsync(expectedContent);
				}
			}

			[Theory]
			[InlineData(HttpStatusCode.Accepted, true)]
			[InlineData(HttpStatusCode.BadGateway, false)]
			public async Task When_responding_with_statusCode_and_stream_and_media_type_it_should_return_response(HttpStatusCode statusCode, bool isSeekable)
			{
				using (var ms = new CanSeekMemoryStream(Encoding.UTF8.GetBytes("content"), isSeekable))
				{
					var request = new HttpRequestMessage();
					var expectedContent = new StringContent("content", Encoding.UTF8, "text/html");

					// Act
					_sut.Respond(statusCode, ms, "text/html; charset=utf-8");
					HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

					// Assert
					await actualResponse.Should()
						.HaveStatusCode(statusCode)
						.And.HaveContentAsync(expectedContent);

					// Second assert, to test we can read stream twice, even if not seekable.
					await actualResponse.Should()
						.HaveStatusCode(statusCode)
						.And.HaveContentAsync(expectedContent);
				}
			}

			[Fact]
			public void When_responding_with_null_stream_it_should_throw()
			{
				// Act
				Action act = () => _sut.Respond((Stream)null);

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.WithParamName("streamContent");
			}

			[Fact]
			public void When_responding_with_stream_and_null_media_type_it_should_not_throw()
			{
				// Act
				Action act = () => _sut.Respond(Stream.Null, (MediaTypeHeaderValue)null);

				// Assert
				act.Should().NotThrow();
			}

			[Fact]
			public void When_responding_with_not_readable_stream_it_should_throw()
			{
				var streamMock = new Mock<Stream>();
				streamMock.Setup(m => m.CanRead).Returns(false);

				// Act
				Action act = () => _sut.Respond(streamMock.Object);

				// Assert
				act.Should()
					.Throw<ArgumentException>()
					.WithParamName("streamContent")
					.WithMessage("Cannot read from stream.*");
			}
		}

		public class WithGeneralContent : IRespondsExtensionsTests
		{
			[Fact]
			public async Task When_responding_with_httpContent_it_should_return_response()
			{
				var httpContent = new StringContent("content");
				var request = new HttpRequestMessage();
				var expectedContent = new StringContent("content");

				// Act
				_sut.Respond(httpContent);
				HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

				// Assert
				await actualResponse.Should()
					.HaveStatusCode(HttpStatusCode.OK)
					.And.HaveContentAsync(expectedContent);
			}

			[Theory]
			[InlineData(HttpStatusCode.Accepted)]
			[InlineData(HttpStatusCode.BadRequest)]
			public async Task When_responding_with_statusCode_and_httpContent_it_should_return_response(HttpStatusCode statusCode)
			{
				var httpContent = new StringContent("content");
				var request = new HttpRequestMessage();
				var expectedContent = new StringContent("content");

				// Act
				_sut.Respond(statusCode, httpContent);
				HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

				// Assert
				await actualResponse.Should()
					.HaveStatusCode(statusCode)
					.And.HaveContentAsync(expectedContent);
			}

			[Fact]
			public void When_responding_with_null_httpContent_it_should_throw()
			{
				// Act
				Action act = () => _sut.Respond((HttpContent)null);

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.WithParamName("content");
			}

			[Theory]
			[InlineData(HttpStatusCode.Accepted)]
			[InlineData(HttpStatusCode.BadRequest)]
			public async Task When_responding_with_statusCode_it_should_return_response(HttpStatusCode statusCode)
			{
				var request = new HttpRequestMessage();

				// Act
				_sut.Respond(statusCode);
				HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

				// Assert
				actualResponse.Should().HaveStatusCode(statusCode);
			}

			[Fact]
			public async Task When_responding_with_string_content_it_should_return_response()
			{
				var request = new HttpRequestMessage();

				// Act
				_sut.Respond("content");
				HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

				// Assert
				await actualResponse.Should()
					.HaveStatusCode(HttpStatusCode.OK)
					.And.HaveContentAsync("content");
			}

			[Theory]
			[InlineData(HttpStatusCode.Accepted)]
			[InlineData(HttpStatusCode.BadRequest)]
			public async Task When_responding_with_statusCode_and_string_content_it_should_return_response(HttpStatusCode statusCode)
			{
				var request = new HttpRequestMessage();

				// Act
				_sut.Respond(statusCode, "content");
				HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

				// Assert
				await actualResponse.Should()
					.HaveStatusCode(statusCode)
					.And.HaveContentAsync("content");
			}

			[Fact]
			public void When_responding_with_null_string_content_it_should_throw()
			{
				// Act
				Action act = () => _sut.Respond((string)null);

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.WithParamName("content");
			}

			[Fact]
			public async Task When_responding_with_string_content_and_media_type_it_should_return_response()
			{
				const string mediaType = "my/mediatype";
				const string expectedMediaType = "my/mediatype; charset=utf-8";
				var request = new HttpRequestMessage();

				// Act
				_sut.Respond("content", mediaType);
				HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

				// Assert
				actualResponse.Should()
					.HaveStatusCode(HttpStatusCode.OK)
					.And.HaveContentType(expectedMediaType);
			}

			[Fact]
			public void When_responding_with_string_content_and_null_media_type_it_should_not_throw()
			{
				// Act
				Action act = () => _sut.Respond("content", (string)null);

				// Assert
				act.Should().NotThrow();
			}

			[Fact]
			public void When_responding_with_string_content_and_null_mediaType_it_should_not_throw()
			{
				// Act
				Action act = () => _sut.Respond("content", (MediaTypeHeaderValue)null);

				// Assert
				act.Should().NotThrow();
			}
		}

		public class NullArgumentTests : IRespondsExtensionsTests
		{
			[Theory]
			[MemberData(nameof(TestCases))]
			public void Given_null_argument_when_executing_method_it_should_throw(params object[] args)
			{
				NullArgumentTest.Execute(args);
			}

			public static IEnumerable<object[]> TestCases()
			{
				var streamMock = new Mock<Stream> { CallBase = true };
				streamMock.SetReturnsDefault(true);
				using var content = new StringContent("");
				IResponds<IResponseResult> responds = Mock.Of<IResponds<IResponseResult>>();

				DelegateTestCase[] testCases = {
					DelegateTestCase.Create<IResponds<IResponseResult>, Func<HttpResponseMessage>, IResponseResult>(
						IRespondsExtensions.Respond,
						responds,
						() => new HttpResponseMessage()),
					DelegateTestCase.Create<IResponds<IResponseResult>, Func<HttpRequestMessage, HttpResponseMessage>, IResponseResult>(
						IRespondsExtensions.Respond,
						responds,
						_ => new HttpResponseMessage()),
					DelegateTestCase.Create(
						IRespondsExtensions.RespondUsing<FakeResponseStrategy, IResponseResult>,
						responds),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						HttpStatusCode.OK),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						"test content"),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						HttpStatusCode.OK,
						"test content"),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						"test content",
						(string)null),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						HttpStatusCode.OK,
						"test content",
						(string)null),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						"test content",
						(MediaTypeHeaderValue)null),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						HttpStatusCode.OK,
						"test content",
						(MediaTypeHeaderValue)null),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						"test content",
						(Encoding)null,
						(string)null),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						HttpStatusCode.OK,
						"test content",
						(Encoding)null,
						(string)null),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						streamMock.Object),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						streamMock.Object,
						(string)null),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						HttpStatusCode.OK,
						streamMock.Object),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						HttpStatusCode.OK,
						streamMock.Object,
						(string)null),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						streamMock.Object,
						(MediaTypeHeaderValue)null),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						HttpStatusCode.OK,
						streamMock.Object,
						(MediaTypeHeaderValue)null),
					DelegateTestCase.Create<IResponds<IResponseResult>, HttpStatusCode, Func<Stream>, MediaTypeHeaderValue, IResponseResult>(
						IRespondsExtensions.Respond,
						responds,
						HttpStatusCode.OK,
						() => streamMock.Object,
						null),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						content),
					DelegateTestCase.Create(
						IRespondsExtensions.Respond,
						responds,
						HttpStatusCode.OK,
						content),
					DelegateTestCase.Create(
						IRespondsExtensions.TimesOut,
						responds),
					DelegateTestCase.Create(
						IRespondsExtensions.TimesOutAfter,
						responds,
						1),
					DelegateTestCase.Create(
						IRespondsExtensions.TimesOutAfter,
						responds,
						TimeSpan.FromMilliseconds(1)),
				};

				return testCases.SelectMany(tc => tc.GetNullArgumentTestCases());
			}

			private class FakeResponseStrategy : IResponseStrategy
			{
				public Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}
