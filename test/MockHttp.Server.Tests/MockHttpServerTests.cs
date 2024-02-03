using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using MockHttp.Fixtures;
using MockHttp.FluentAssertions;
using MockHttp.Http;
using Xunit.Abstractions;

namespace MockHttp;

[Collection(nameof(DisableParallelization))]
public sealed class MockHttpServerTests : IClassFixture<MockHttpServerFixture>, IDisposable
{
    private static readonly Uri BaseUri = new("http://127.0.0.1:0");

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
                .Body("request-content")
                .ContentType(MediaTypes.PlainText, Encoding.ASCII)
            )
            .Respond(() => new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("Some content", Encoding.UTF8, MediaTypes.Html),
                Headers =
                {
                    { "return-test", "return-value" }
                }
            })
            .Verifiable();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "test/wtf/");
        request.Content = new StringContent("request-content", Encoding.ASCII, MediaTypes.PlainText);
        request.Headers.Add("test", "value");

        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.Accepted);
        response.Should().HaveContentType(MediaTypes.Html, Encoding.UTF8);
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
                .Respond(with => with.StatusCode(HttpStatusCode.BadGateway))
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
                .Respond(with => with.Body("has content"))
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
                .Respond((ctx, with) => with
                    .Header(headerKey, ctx.Request.Headers.GetValues(headerKey))
                )
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
                .Body("request-content", Encoding.ASCII)
            )
            .Respond(() => new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("Response content", Encoding.UTF8, MediaTypes.Html),
                Headers =
                {
                    { "return-test", "return-value" }
                }
            })
            .Verifiable();

        // Act
#pragma warning disable CS0618
#pragma warning disable SYSLIB0014 // Type or member is obsolete - justification: testing other API's
        var request = WebRequest.Create($"{_fixture.Server.HostUrl}/web-request");
#pragma warning restore SYSLIB0014 // Type or member is obsolete
#pragma warning restore CS0618
        request.Method = "POST";
        request.Headers.Add("test", "value");
        request.ContentType = MediaTypes.PlainText;
        await using (Stream requestStream = await request.GetRequestStreamAsync())
        {
            requestStream.Write(Encoding.ASCII.GetBytes("request-content"));
        }

        // Assert
        var response = (HttpWebResponse)await request.GetResponseAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.ContentType.Should().Be($"{MediaTypes.Html}; charset=utf-8");
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
        _fixture.Handler.Fallback.Respond(with => with
            .StatusCode(HttpStatusCode.BadRequest)
            .Body("Should return fallback.")
        );

        using HttpClient client = _fixture.Server.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("");

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        await response.Should().HaveContentAsync("Should return fallback.");
        response.ReasonPhrase.Should().Be("Bad Request");
        await _fixture.Handler.VerifyAsync(_ => { }, IsSent.Once);
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
        response.Should().HaveContentType(MediaTypes.PlainText, Encoding.UTF8);
        await response.Should().HaveContentAsync(expectedErrorMessage + Environment.NewLine + ex);
        response.ReasonPhrase.Should().Be(expectedErrorMessage);
        _fixture.Handler.Verify();
    }

    [Fact]
    public async Task Given_that_request_is_configured_with_server_timeout_when_sending_it_should_respond_with_request_timed_out()
    {
        _fixture.Handler
            .When(matching => matching.Method(HttpMethod.Get))
            .Respond(with => with.ServerTimeout())
            .Verifiable();

        using HttpClient client = _fixture.Server.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("");

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.RequestTimeout);
        _fixture.Handler.Verify();
    }

    [Fact]
    public void When_creating_server_with_null_handler_it_should_throw()
    {
        MockHttpHandler? mockHttpHandler = null;

        // Act
        Func<MockHttpServer> act = () => new MockHttpServer(mockHttpHandler!, BaseUri);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(mockHttpHandler));
    }

    [Fact]
    public async Task When_creating_and_starting_server_with_null_logger_it_should_not_throw()
    {
        ILoggerFactory? loggerFactory = null;

        // Act
        Func<MockHttpServer> act = () => new MockHttpServer(new MockHttpHandler(), loggerFactory, BaseUri);

        // Assert
        MockHttpServer server = act.Should().NotThrow().Which;
        await using (server)
        {
            Func<Task> act2 = () => server.StartAsync();
            await act2.Should().NotThrowAsync();
        }
    }

    [Fact]
    [Obsolete("Removed in next major version.")]
    public void When_creating_server_with_null_host_it_should_throw()
    {
        string? hostUrl = null;

        // Act
#pragma warning disable CS8604
        Func<MockHttpServer> act = () => new MockHttpServer(new MockHttpHandler(), hostUrl);
#pragma warning restore CS8604

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(hostUrl));
    }

    [Fact]
    [Obsolete("Removed in next major version.")]
    public void When_creating_server_with_invalid_host_it_should_throw()
    {
        const string hostUrl = "relative/uri/is/invalid";

        // Act
        // ReSharper disable once ExpressionIsAlwaysNull
        Func<MockHttpServer> act = () => new MockHttpServer(new MockHttpHandler(), hostUrl);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName(nameof(hostUrl));
    }

    [Fact]
    [Obsolete("Removed in next major version.")]
    public async Task When_creating_server_with_absolute_uri_it_should_not_throw_and_take_host_from_url()
    {
        var hostUrl = new Uri("https://relative:789/uri/is/invalid");
        const string expectedHostUrl = "https://relative:789";

        // Act
        // ReSharper disable once ExpressionIsAlwaysNull
        Func<MockHttpServer> act = () => new MockHttpServer(new MockHttpHandler(), hostUrl);

        // Assert
        MockHttpServer server = act.Should().NotThrow().Which;
        await using (server)
        {
            act.Should().NotThrow().Which.HostUrl.Should().Be(expectedHostUrl);
        }
    }

    [Fact]
    public async Task When_creating_server_with_absolute_uri_it_should_not_throw_and_take_host_from_uri()
    {
        var hostUrl = new Uri("https://relative:789/uri/is/invalid");
        var expectedHostUrl = new Uri("https://relative:789");

        // Act
        // ReSharper disable once ExpressionIsAlwaysNull
        Func<MockHttpServer> act = () => new MockHttpServer(new MockHttpHandler(), hostUrl);

        // Assert
        MockHttpServer server = act.Should().NotThrow().Which;
        await using (server)
        {
            act.Should().NotThrow().Which.HostUri.Should().Be(expectedHostUrl);
        }
    }

    [Fact]
    public async Task Given_that_server_is_started_when_starting_again_it_should_not_throw()
    {
        var server = new MockHttpServer(_fixture.Handler, BaseUri);
        await server.StartAsync();
        server.IsStarted.Should().BeTrue();

        // Act
        Func<Task> act = () => server.StartAsync();

        // Assert
        await using (server)
        {
            await act.Should().NotThrowAsync();
            server.IsStarted.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Given_that_server_is_started_when_disposing_it_should_stop()
    {
        var server = new MockHttpServer(_fixture.Handler, BaseUri);
        await server.StartAsync();
        server.IsStarted.Should().BeTrue();

        // Act
        Func<Task> act = () => server.DisposeAsync().AsTask();

        // Assert
        await using (server)
        {
            await act.Should().NotThrowAsync();
            server.IsStarted.Should().BeFalse();
        }
    }

    [Fact]
    public async Task Given_that_server_is_stopped_when_stopping_again_it_should_not_throw()
    {
        var server = new MockHttpServer(_fixture.Handler, BaseUri);
        await server.StopAsync();

        // Act
        Func<Task> act = () => server.StopAsync();

        // Assert
        await using (server)
        {
            await act.Should().NotThrowAsync();
        }
    }

    [Fact]
    public async Task Given_that_server_is_disposed_when_disposing_again_it_should_not_throw()
    {
        var server = new MockHttpServer(_fixture.Handler, BaseUri);
        await server.DisposeAsync();

        // Act
        Func<Task> act = () => server.DisposeAsync().AsTask();

        // Assert
        await using (server)
        {
            await act.Should().NotThrowAsync();
        }
    }

    [Fact]
    public async Task Given_that_server_is_stopped_when_disposing_it_should_not_throw()
    {
        var server = new MockHttpServer(_fixture.Handler, BaseUri);
        await server.StopAsync();

        // Act
        Func<Task> act = () => server.DisposeAsync().AsTask();

        // Assert
        await using (server)
        {
            await act.Should().NotThrowAsync();
        }
    }

    [Fact]
    public async Task Given_that_server_is_disposed_when_stopping_it_should_throw()
    {
        var server = new MockHttpServer(_fixture.Handler, BaseUri);
        await server.DisposeAsync();

        // Act
        Func<Task> act = () => server.StopAsync();

        // Assert
        await using (server)
        {
            (await act.Should().ThrowAsync<ObjectDisposedException>())
                .Which.ObjectName.Should().Be(typeof(MockHttpServer).FullName);
        }
    }

    [Fact]
    public async Task Given_that_server_is_disposed_when_starting_it_should_throw()
    {
        var server = new MockHttpServer(_fixture.Handler, BaseUri);
        await server.DisposeAsync();

        // Act
        Func<Task> act = () => server.StartAsync();

        // Assert
        await using (server)
        {
            (await act.Should().ThrowAsync<ObjectDisposedException>())
                .Which.ObjectName.Should().Be(typeof(MockHttpServer).FullName);
        }
    }

    [Fact]
    public async Task Given_that_server_is_disposed_when_creating_client_it_should_throw()
    {
        var server = new MockHttpServer(_fixture.Handler, BaseUri);
        await server.DisposeAsync();

        // Act
        Func<HttpClient> act = () => server.CreateClient();

        // Assert
        await using (server)
        {
            act.Should().Throw<ObjectDisposedException>()
                .Which.ObjectName.Should().Be(typeof(MockHttpServer).FullName);
        }
    }

    [Fact]
    public async Task Given_that_server_is_disposed_when_getting_hostUri_it_should_throw()
    {
        var server = new MockHttpServer(_fixture.Handler, BaseUri);
        await server.DisposeAsync();

        // Act
        Func<Uri> act = () => server.HostUri;

        // Assert
        await using (server)
        {
            act.Should().Throw<ObjectDisposedException>()
                .Which.ObjectName.Should().Be(typeof(MockHttpServer).FullName);
        }
    }

    [Fact]
    public void When_creating_server_handler_it_should_set_property()
    {
        var handler = new MockHttpHandler();

        // Act
        var server = new MockHttpServer(handler, BaseUri);

        // Assert
        server.Handler.Should().Be(handler);
    }
}
