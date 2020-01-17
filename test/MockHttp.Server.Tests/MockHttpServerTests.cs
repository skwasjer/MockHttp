using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockHttp.Fixtures;
using MockHttp.FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace MockHttp
{
	public class MockHttpServerTests : IClassFixture<MockHttpServerFixture>, IDisposable
	{
		private readonly MockHttpServerFixture _fixture;
		private readonly ITestOutputHelper _testOutputHelper;

		public MockHttpServerTests(MockHttpServerFixture fixture, ITestOutputHelper testOutputHelper)
		{
			_fixture = fixture;
			_fixture.Reset();
			_testOutputHelper = testOutputHelper;
		}

		public void Dispose()
		{
			_fixture.LogServerTrace(_testOutputHelper);
		}

		[Fact]
		public async Task Given_mocked_server_when_sending_complete_request_it_should_respond()
		{
			using HttpClient client = _fixture.Server.CreateClient();

			_fixture.Handler
				.When(matching => matching
					.RequestUri("test/wtf/")
					.Header("test", "value")
					.Method(HttpMethod.Post)
					.Content("request-content")
					.ContentType("text/plain", Encoding.ASCII)
				)
				.Respond(() => new HttpResponseMessage(HttpStatusCode.Accepted)
				{
					Content = new StringContent("Some content", Encoding.UTF8, "text/html"),
					Headers =
					{
						{ "return-test", "return-value" }
					}
				})
				.Verifiable();

			// Act
			using var request = new HttpRequestMessage(HttpMethod.Post, "test/wtf/")
			{
				Content = new StringContent("request-content", Encoding.ASCII, "text/plain"),
				Headers =
				{
					{ "test", "value"}
				}
			};

			HttpResponseMessage response = await client.SendAsync(request);

			// Assert
			response.Should().HaveStatusCode(HttpStatusCode.Accepted);
			response.Should().HaveContentType("text/html");
			response.Should().HaveHeader("return-test", "return-value");
			await response.Should().HaveContentAsync("Some content");
			_fixture.Handler.Verify();
		}

		[Theory]
		[MemberData(nameof(RequestResponseTestCases))]
		public async Task Given_configured_request_when_sending_it_should_respond_with_expected_response(Action<MockHttpHandler> configureHandler, HttpRequestMessage request, Func<HttpResponseMessage, Task> assertResponse)
		{
			using HttpClient client = _fixture.Server.CreateClient();

			configureHandler(_fixture.Handler);

			// Act
			using (request)
			{
				HttpResponseMessage response = await client.SendAsync(request);

				// Assert
				await assertResponse(response);
				_fixture.Handler.Verify();
			}
		}

		public static IEnumerable<object[]> RequestResponseTestCases()
		{
			// By method, returning status code.
			yield return new object[]
			{
				(Action<MockHttpHandler>)(m => m
					.When(matching => matching.Method(HttpMethod.Post))
					.Respond(HttpStatusCode.BadGateway)
				),
				new HttpRequestMessage(HttpMethod.Post, ""),
				(Func<HttpResponseMessage, Task>)(response =>
				{
					response.Should().HaveStatusCode(HttpStatusCode.BadGateway);
					return Task.CompletedTask;
				})
			};

			// By wildcard path & query string, returning content
			yield return new object[]
			{
				(Action<MockHttpHandler>)(m => m
					.When(matching => matching
						.RequestUri("*path/child*")
						.QueryString("?key=value")
					)
					.Respond("has content")
				),
				new HttpRequestMessage(HttpMethod.Get, "/path/child/?key=value"),
				(Func<HttpResponseMessage, Task>)(async response =>
				{
					await response.Should().HaveContentAsync("has content");
				})
			};

			// By header.
			const string headerKey = "X-Correlation-ID";
			const string headerValue = "my-id";
			yield return new object[]
			{
				(Action<MockHttpHandler>)(m => m
					.When(matching => matching
						.Header(headerKey, headerValue)
					)
					.Respond((r) => new HttpResponseMessage
					{
						Headers =
						{
							{ headerKey, r.Headers.GetValues(headerKey) }
						}
					})
				),
				new HttpRequestMessage
				{
					Headers =
					{
						{ headerKey, headerValue }
					}
				},
				(Func<HttpResponseMessage, Task>)(response =>
				{
					response.Should().HaveHeader(headerKey, headerValue);
					return Task.CompletedTask;
				})
			};
		}

		[Fact]
		public async Task When_using_other_API_like_webRequest_it_should_respond_correctly()
		{
			_fixture.Handler
				.When(matching => matching
					.RequestUri("web-request")
					.Method(HttpMethod.Post)
					.Content("request-content", Encoding.ASCII)
				)
				.Respond(() => new HttpResponseMessage(HttpStatusCode.Accepted)
				{
					Content = new StringContent("Response content", Encoding.UTF8, "text/html"),
					Headers =
					{
						{ "return-test", "return-value" }
					}
				})
				.Verifiable();

			// Act
			var request = WebRequest.Create($"{_fixture.Server.HostUrl}/web-request");
			request.Method = "POST";
			request.Headers.Add("test", "value");
			request.ContentType = "text/plain";
			using (Stream requestStream = await request.GetRequestStreamAsync())
			{
				requestStream.Write(Encoding.ASCII.GetBytes("request-content"));
			}

			// Assert
			HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
			response.StatusCode.Should().Be(HttpStatusCode.Accepted);
			response.ContentType.Should().Be("text/html; charset=utf-8");
			using (var sr = new StreamReader(response.GetResponseStream()))
			{
				(await sr.ReadToEndAsync()).Should().Be("Response content");
			}

			_fixture.Handler.Verify();
		}

		[Fact]
		public async Task Given_unmocked_request_when_sending_it_should_respond_with_fallback_response()
		{
			_fixture.Handler.Reset();
			_fixture.Handler.Fallback.Respond(HttpStatusCode.BadRequest, "Should return fallback.");

			using HttpClient client = _fixture.Server.CreateClient();

			// Act
			HttpResponseMessage response = await client.GetAsync("");

			// Assert
			response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
			await response.Should().HaveContentAsync("Should return fallback.");
			response.ReasonPhrase.Should().Be("Bad Request");
			_fixture.Handler.Verify(matching => {}, IsSent.Once);
		}

		[Fact]
		public async Task Given_request_is_configured_to_throw_when_sending_it_should_respond_with_internal_server_error()
		{
			const string expectedErrorMessage = "MockHttpServer failed to handle request. Please verify your mock setup is correct.";
			var ex = new InvalidOperationException("Mock throws.");
			_fixture.Handler
				.When(matching => matching
					.Method(HttpMethod.Get)
				)
				.Throws(ex)
				.Verifiable();

			using HttpClient client = _fixture.Server.CreateClient();

			// Act
			HttpResponseMessage response = await client.GetAsync("");

			// Assert
			response.Should().HaveStatusCode(HttpStatusCode.InternalServerError);
			await response.Should().HaveContentAsync(expectedErrorMessage + Environment.NewLine + ex);
			response.ReasonPhrase.Should().Be(expectedErrorMessage);
			_fixture.Handler.Verify();
		}

		[Fact]
		public void When_creating_server_with_null_handler_it_should_throw()
		{
			MockHttpHandler mockHttpHandler = null;

			// Act
			// ReSharper disable once ObjectCreationAsStatement
			// ReSharper disable once ExpressionIsAlwaysNull
			Action act = () => new MockHttpServer(mockHttpHandler, "");

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName(nameof(mockHttpHandler));
		}

		[Fact]
		public void When_creating_and_starting_server_with_null_logger_it_should_not_throw()
		{
			ILoggerFactory loggerFactory = null;

			// Act
			// ReSharper disable once ObjectCreationAsStatement
			// ReSharper disable once ExpressionIsAlwaysNull
			Func<MockHttpServer> act = () => new MockHttpServer(new MockHttpHandler(), loggerFactory, "http://127.0.0.1");

			// Assert
			using MockHttpServer server = act.Should().NotThrow().Which;
			Func<Task> act2 = () => server.StartAsync();
			act2.Should().NotThrow();
		}

		[Fact]
		public void When_creating_server_with_null_host_it_should_throw()
		{
			string hostUrl = null;

			// Act
			// ReSharper disable once ObjectCreationAsStatement
			// ReSharper disable once ExpressionIsAlwaysNull
			Action act = () => new MockHttpServer(new MockHttpHandler(), hostUrl);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName(nameof(hostUrl));
		}

		[Fact]
		public void When_creating_server_with_invalid_host_it_should_throw()
		{
			const string hostUrl = "relative/uri/is/invalid";

			// Act
			// ReSharper disable once ObjectCreationAsStatement
			// ReSharper disable once ExpressionIsAlwaysNull
			Action act = () => new MockHttpServer(new MockHttpHandler(), hostUrl);

			// Assert
			act.Should().Throw<ArgumentException>().WithParamName(nameof(hostUrl));
		}

		[Fact]
		public void When_creating_server_with_absolute_uri_it_should_not_throw_and_take_host_from_uri()
		{
			const string hostUrl = "https://relative:789/uri/is/invalid";
			const string expectedHostUrl = "https://relative:789";

			// Act
			// ReSharper disable once ObjectCreationAsStatement
			// ReSharper disable once ExpressionIsAlwaysNull
			Func<MockHttpServer> act = () => new MockHttpServer(new MockHttpHandler(), hostUrl);

			// Assert
			act.Should().NotThrow().Which.HostUrl.Should().Be(expectedHostUrl);
		}

		[Fact]
		public async Task Given_server_is_started_when_starting_again_it_should_throw()
		{
			using var server = new MockHttpServer(_fixture.Handler, "http://127.0.0.1");
			await server.StartAsync();
			server.IsStarted.Should().BeTrue();

			// Act
			Func<Task> act = () => server.StartAsync();

			// Assert
			act.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void Given_server_is_not_started_when_stopped_it_should_throw()
		{
			using var server = new MockHttpServer(_fixture.Handler, "http://127.0.0.1");
			server.IsStarted.Should().BeFalse();

			// Act
			Func<Task> act = () => server.StopAsync();

			// Assert
			act.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void When_creating_server_handler_it_should_set_property()
		{
			var handler = new MockHttpHandler();

			// Act
			var server = new MockHttpServer(handler, "http://127.0.0.1");

			// Assert
			server.Handler.Should().Be(handler);
		}
	}
}
