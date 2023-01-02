using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions.Extensions;
using MockHttp.FluentAssertions;
using MockHttp.Http;
using MockHttp.Language;
using MockHttp.Language.Flow;
using Newtonsoft.Json;

namespace MockHttp;

public class MockHttpHandlerTests : IDisposable
{
    private readonly MockHttpHandler _sut;
    private readonly HttpClient _httpClient;

    public MockHttpHandlerTests()
    {
        _sut = new MockHttpHandler();
        _httpClient = new HttpClient(_sut) { BaseAddress = new Uri("http://0.0.0.0") };
    }

    public void Dispose()
    {
        _sut?.Dispose();
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Exception that we can test <see cref="IThrows{TResult}" /> logic with.
    /// </summary>
    private class TestableException : Exception
    {
    }

    [Fact]
    public async Task Given_request_is_configured_to_throw_when_sending_request_should_throw_exception()
    {
        _sut.When(_ => { })
            .Throws<TestableException>()
            .Verifiable();

        // Act
        Func<Task<HttpResponseMessage>> act = () => _httpClient.GetAsync("");

        // Assert
        await act.Should().ThrowExactlyAsync<TestableException>();
        _sut.Verify(_ => { }, IsSent.Once);
        _sut.VerifyNoOtherRequests();
    }

    [Fact]
    public async Task Given_request_is_configured_to_throw_specific_exception_when_sending_request_should_throw_exception()
    {
        var exception = new TestableException();
        _sut.When(_ => { })
            .Throws(exception)
            .Verifiable();

        // Act
        Func<Task<HttpResponseMessage>> act = () => _httpClient.GetAsync("");

        // Assert
        (await act.Should()
                .ThrowAsync<TestableException>())
            .Which.Should()
            .Be(exception);
        _sut.Verify(_ => { }, IsSent.Once);
        _sut.VerifyNoOtherRequests();
    }

    [Fact]
    public async Task Given_request_is_configured_to_return_response_when_sending_request_should_return_response()
    {
        const string data = "data";
        _sut.When(_ => { })
            .Respond(with => with.Body(new StringContent(data)));

        // Act
        HttpResponseMessage actualResponse = await _httpClient.GetAsync("");

        // Assert
        actualResponse.Should().NotBeNull();
        actualResponse.Content.Should().NotBeNull();
        await actualResponse.Should().HaveContentAsync(data);
        _sut.Verify(_ => { }, IsSent.Once);
        _sut.VerifyNoOtherRequests();
    }

    [Fact]
    public async Task Given_no_request_is_configured_when_sending_request_should_return_fallback_response()
    {
        // Act
        HttpResponseMessage actualResponse = await _httpClient.GetAsync("");

        // Assert
        actualResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);
        actualResponse.ReasonPhrase.Should().Be("No request is configured, returning default response.");
        _sut.Verify(_ => { }, IsSent.Once);
        _sut.VerifyNoOtherRequests();
    }

    [Fact]
    public async Task Given_no_request_is_configured_and_custom_fallback_is_configured_when_sending_request_should_return_fallback_response()
    {
        _sut.Fallback.Respond(with => with.StatusCode(HttpStatusCode.BadRequest));

        // Act
        HttpResponseMessage actualResponse = await _httpClient.GetAsync("");

        // Assert
        actualResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Given_no_request_is_configured_and_custom_fallback_is_configured_to_throw_when_sending_request_should_throw()
    {
        _sut.Fallback.Throws<TestableException>();

        // Act
        Func<Task> act = () => _httpClient.GetAsync("");

        // Assert
        await act.Should().ThrowAsync<TestableException>();
    }

    [Fact]
    public async Task Given_no_request_is_configured_and_custom_fallback_is_configured_to_throw_specific_exception_when_sending_request_should_throw()
    {
        var ex = new TestableException();
        _sut.Fallback.Throws(ex);

        // Act
        Func<Task> act = () => _httpClient.GetAsync("");

        // Assert
        (await act.Should().ThrowAsync<TestableException>()).Which.Should().Be(ex);
    }

    [Fact]
    public async Task Given_a_callback_is_configured_when_sending_request_should_invoke_callback_before_sending_request()
    {
        bool callbackCalled = false;
        const string expectedReason = "Callback is called = True";
        _sut.When(_ => { })
            .Callback(() => callbackCalled = true)
            .Respond(() => new HttpResponseMessage { ReasonPhrase = $"Callback is called = {callbackCalled}" });

        // Act
        HttpResponseMessage actualResponse = await _httpClient.GetAsync("");

        // Assert
        actualResponse.ReasonPhrase.Should().Be(expectedReason, "the callback should be called before we returned the response");
        _sut.Verify(_ => { }, IsSent.Once);
        _sut.VerifyNoOtherRequests();
    }

    [Fact]
    public async Task Given_no_response_is_configured_when_sending_request_should_throw()
    {
        _sut.When(_ => { });

        // Act
        Func<Task<HttpResponseMessage>> act = () => _httpClient.GetAsync("");

        // Assert
        await act.Should()
            .ThrowAsync<HttpMockException>("the mock is configured without a response")
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
        _sut.VerifyNoOtherRequests();
    }

    [Fact]
    public async Task Given_expectation_fails_when_verifying_should_throw()
    {
        _sut.When(matching => matching.Method(HttpMethod.Post))
            .Respond(with => with.StatusCode(HttpStatusCode.OK))
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
            .Respond(with => with.StatusCode(HttpStatusCode.OK))
            .Verifiable();

        await _httpClient.GetAsync("");

        // Act
        Action verifyGet = () => _sut.Verify();

        // Assert
        verifyGet.Should().NotThrow<HttpMockException>();
        _sut.VerifyNoOtherRequests();
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
            matching => matching.RequestUri("**/url"),
            isSent
        );

        // Assert
        verifyGet.Should().NotThrow<HttpMockException>();
        _sut.VerifyNoOtherRequests();
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
            matching => matching.RequestUri("**/url"),
            isSent
        );

        // Assert
        verifyGet.Should()
            .Throw<HttpMockException>()
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
    public async Task Given_a_request_expectation_when_sending_requests_it_should_correctly_match_and_verify_the_response()
    {
        var postObject = new
        {
            Field1 = true,
            Field2 = "some text",
            Field3 = DateTime.UtcNow
        };
        string jsonPostContent = JsonConvert.SerializeObject(postObject);
        var lastModified = new DateTime(2018, 4, 12, 7, 22, 43, DateTimeKind.Local);
        var postContent = new StringContent(jsonPostContent, Encoding.UTF8, MediaTypes.Json) { Headers = { LastModified = lastModified } };

        // ReSharper disable once JoinDeclarationAndInitializer
        Version version;
#if NETCOREAPP3_1 || NET6_0_OR_GREATER
        version = _httpClient.DefaultRequestVersion;
#else
#if NET5_0_OR_GREATER
        version = new Version(2, 0);
#else
        version = new Version(1, 1);
#endif
#endif

        _sut
            .When(matching => matching
                .RequestUri("http://0.0.0.1/*/action*")
                .QueryString("test", "$%^&*")
                .QueryString("test2=value")
                .Method("POST")
                .Body(jsonPostContent)
                .PartialBody(jsonPostContent.Substring(10))
                .ContentType($"{MediaTypes.Json}; charset=utf-8")
                .BearerToken()
                .Header("Content-Length", jsonPostContent.Length)
                .Header("Last-Modified", lastModified)
                .Version(version)
                .Any(any => any
                    .RequestUri("not-matching")
                    .RequestUri("**controller**")
                )
                .Where(r => 0 < r.Version.Major)
            )
            .Callback(() =>
            {
            })
            .Respond(with => with
                .StatusCode(HttpStatusCode.Accepted)
                .Body(JsonConvert.SerializeObject(new { firstName = "John", lastName = "Doe" }))
                .Latency(NetworkLatency.TwoG)
            )
            .Verifiable();

        // Act
        await _httpClient.GetAsync("http://0.0.0.1/controller/action?test=1");
        var req = new HttpRequestMessage(HttpMethod.Post, "http://0.0.0.1/controller/action?test=%24%25^%26*&test2=value")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", "some-token") },
            Content = postContent,
            Version = version
        };
        HttpResponseMessage response = await _httpClient.SendAsync(req);

        // Assert
        _sut.Verify(matching => matching.RequestUri("**/controller/**"), IsSent.Exactly(2), "we sent it");
#if !NETFRAMEWORK
        await _sut.VerifyAsync(matching => matching.Body(jsonPostContent), IsSent.Once, "we sent it");
#endif
        _sut.Verify();
        _sut.VerifyNoOtherRequests();

        await response.Should()
            .HaveStatusCode(HttpStatusCode.Accepted)
            .And.HaveContentAsync(JsonConvert.SerializeObject(
                new { firstName = "John", lastName = "Doe" })
            );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_stream_response_when_sending_requests_it_should_buffer_response(bool isSeekableStream)
    {
        const string data = "data";
        byte[] buffer = Encoding.UTF8.GetBytes(data);
        using Stream stream = new CanSeekMemoryStream(buffer, isSeekableStream);
        _sut.When(_ => { })
            .Respond(with => with
                .Body(stream)
                .ContentType(MediaTypes.PlainText)
            )
            .Verifiable();

        // Act
        HttpResponseMessage firstResponse = await _httpClient.GetAsync("url");
        HttpResponseMessage? secondResponse = null;
        Func<Task<HttpResponseMessage>> act = async () => secondResponse = await _httpClient.GetAsync("url");

        // Assert
        await act.Should().NotThrowAsync();

        _sut.Verify(_ => { }, IsSent.Exactly(2));
        secondResponse.Should().NotBeNull();
        firstResponse.Content.Should().NotBeSameAs(secondResponse!.Content);
        await (await firstResponse.Should()
                .HaveContentAsync(await secondResponse.Content.ReadAsStringAsync()))
            .And.HaveContentAsync(data);

        _sut.VerifyNoOtherRequests();
    }

    [Fact]
    public async Task Given_httpContent_response_when_sending_requests_it_should_not_throw_for_second_request_and_return_same_content()
    {
        const string data = "data";
        using HttpContent httpContent = new StringContent(data);
        _sut.When(_ => { })
            .Respond(with => with
                .Body(httpContent)
            )
            .Verifiable();

        // Act
        HttpResponseMessage firstResponse = await _httpClient.GetAsync("url");
        HttpResponseMessage? secondResponse = null;
        Func<Task<HttpResponseMessage>> act = async () => secondResponse = await _httpClient.GetAsync("url");

        // Assert
        await act.Should().NotThrowAsync();

        await _sut.VerifyAsync(_ => { }, IsSent.Exactly(2));
        firstResponse.Content.Should().BeOfType<ByteArrayContent>("a buffered copy is created and returned for all responses");
        secondResponse.Should().NotBeNull();
        firstResponse.Content.Should().NotBeSameAs(secondResponse!.Content);
        await (await firstResponse.Should()
                .HaveContentAsync(await secondResponse.Content.ReadAsStringAsync()))
            .And.HaveContentAsync(data);

        _sut.VerifyNoOtherRequests();
    }

    [Fact]
    public async Task Given_request_is_configured_to_time_out_when_sending_request_should_throw()
    {
        _sut.When(_ => { })
            .Respond(with => with
                .ClientTimeout()
            )
            .Verifiable();

        // Act
        Func<Task> act = () => _httpClient.GetAsync("");

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
        _sut.Verify();
    }

    [Fact]
    public async Task Given_request_is_not_verified_when_verifying_no_other_calls_should_throw()
    {
        _sut.When(matching => matching.Method("GET"))
            .Respond(with => with.StatusCode(200))
            .Verifiable();

        await _httpClient.GetAsync("");
        await _httpClient.PutAsync("", new StringContent("data"));
        await _httpClient.PostAsync("", new StringContent("data"));
        await _httpClient.GetAsync("");
        await _httpClient.PutAsync("", new StringContent("data"));

        // Verify GET and PUT via different means, but not POST
        _sut.Verify(matching => matching.Method("PUT"), IsSent.Exactly(2));
        _sut.Verify();

        // Act
        Action act = () => _sut.VerifyNoOtherRequests();

        // Assert
        act.Should()
            .Throw<HttpMockException>("the POST request was not verified")
            .WithMessage("There are 1 unverified requests*Method: POST,*");
    }

    [Fact]
    public async Task Given_request_is_configured_to_have_different_responses_should_return_response_in_sequence()
    {
        _sut.When(_ => { })
            .Respond(with => with.StatusCode(HttpStatusCode.BadRequest))
            .Respond(with => with.StatusCode(HttpStatusCode.BadGateway))
            .Respond(with => with.StatusCode(HttpStatusCode.OK))
            .Verifiable();

        // Act
        HttpResponseMessage response1 = await _httpClient.GetAsync("");
        HttpResponseMessage response2 = await _httpClient.GetAsync("");
        HttpResponseMessage response3 = await _httpClient.GetAsync("");
        HttpResponseMessage response4 = await _httpClient.GetAsync("");

        // Assert
        response1.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        response2.Should().HaveStatusCode(HttpStatusCode.BadGateway);
        response3.Should().HaveStatusCode(HttpStatusCode.OK);
        response4.Should().HaveStatusCode(HttpStatusCode.OK);
        _sut.Verify(matching => matching.Method("GET"), IsSent.Exactly(4));
        _sut.VerifyNoOtherRequests();
    }

    [Fact]
    public async Task Given_request_is_configured_to_fail_and_then_succeed_should_return_response_in_sequence()
    {
        var ex = new Exception("My exception");

        _sut.When(_ => { })
            .Throws(ex)
            .Respond(with => with.ClientTimeout(500.Milliseconds()))
            .Respond(with => with.StatusCode(HttpStatusCode.OK));

        // Act & assert
        Func<Task<HttpResponseMessage>> act1 = () => _httpClient.GetAsync("");
        Func<Task<HttpResponseMessage>> act2 = () => _httpClient.GetAsync("");
        HttpResponseMessage? response3 = null;
        Func<Task<HttpResponseMessage>> act3 = async () => response3 = await _httpClient.GetAsync("");

        // Assert
        (await act1.Should().ThrowAsync<Exception>()).Which.Should().Be(ex);
        await act2.Should().ThrowAsync<TaskCanceledException>();
        await act3.Should().NotThrowAsync();
        response3.Should()
            .NotBeNull()
            .And.HaveStatusCode(HttpStatusCode.OK);

        _sut.Verify(matching => matching.Method("GET"), IsSent.Exactly(3));
        _sut.VerifyNoOtherRequests();
    }

    [Fact]
    public async Task When_sending_requests_in_parallel_should_be_thread_safe()
    {
        const int parallelCount = 20;
        const int nrOfIterations = 20;
        var expectedIndices = new List<int>();

        ISequenceResponseResult responseResult = _sut.When(_ => { });
        // Configure unique response n times.
        for (int i = 0; i < parallelCount * nrOfIterations; i++)
        {
            int requestIndex = i;
            expectedIndices.Add(requestIndex);
            // Each next response returns the counter value as content, so we can assert on this.
            responseResult = responseResult.Respond(with => with.Body(requestIndex.ToString()));
        }

        Task<List<int>> GetParallelTask()
        {
            return Task.Run(async () =>
            {
                await Task.Yield();

                var result = new List<int>();
                for (int i = 0; i < nrOfIterations; i++)
                {
                    HttpResponseMessage response = await _httpClient.GetAsync("");
                    result.Add(int.Parse(await response.Content.ReadAsStringAsync()));
                }

                return result;
            });
        }

        // Act
        IEnumerable<Task<List<int>>> tasks = Enumerable.Range(0, parallelCount).Select(_ => GetParallelTask());
        IEnumerable<int> allResults = (await Task.WhenAll(tasks)).SelectMany(l => l).ToList();

        // Assert
        expectedIndices.Except(allResults).Should().BeEmpty("for each request a corresponding response was configured");
    }

    [Fact]
    public async Task When_resetting_invoked_requests_it_should_reset_sequence()
    {
        HttpStatusCode[] statusCodeSequence = { HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.BadRequest };

        IResponds<IResponseResult> result = _sut.When(_ => { });
        _ = statusCodeSequence.Aggregate(result,
            (current, next) =>
                (IResponds<IResponseResult>)current.Respond(with => with.StatusCode(next))
        );

        // Act
        foreach (HttpStatusCode expectedStatus in statusCodeSequence.SkipLast(1))
        {
            HttpResponseMessage response = await _httpClient.GetAsync("");
            response.Should().HaveStatusCode(expectedStatus);
        }

        _sut.InvokedRequests.Clear();

        // Assert
        foreach (HttpStatusCode expectedStatus in statusCodeSequence)
        {
            HttpResponseMessage response = await _httpClient.GetAsync("");
            response.Should().HaveStatusCode(expectedStatus);
        }
    }

    public class VerifyNullArgTests
    {
        [Theory]
        [MemberData(nameof(TestCases))]
        public void Given_null_argument_when_executing_method_it_should_throw(params object[] args)
        {
            NullArgumentTest.Execute(args);
}

        public static IEnumerable<object[]> TestCases()
        {
            var mockHttp = new MockHttpHandler();
#pragma warning disable IDE0039
            // ReSharper disable once ConvertToLocalFunction
            Action<RequestMatching> matching = _ => { };
#pragma warning restore IDE0039
            Func<IsSent> isSentFunc = IsSent.Once;
            string? because = null;

            DelegateTestCase[] testCases =
            {
                DelegateTestCase.Create(
                    mockHttp.Verify,
                    matching,
                    isSentFunc,
                    because),
                DelegateTestCase.Create(
                    mockHttp.Verify,
                    matching,
                    isSentFunc(),
                    because),
                DelegateTestCase.Create(
                    mockHttp.VerifyAsync,
                    matching,
                    isSentFunc,
                    because),
                DelegateTestCase.Create(
                    mockHttp.VerifyAsync,
                    matching,
                    isSentFunc(),
                    because)
            };

            return testCases.SelectMany(tc => tc.GetNullArgumentTestCases());

        }
    }
}
#if NETFRAMEWORK
internal static class EnumerableExtensions
{
    // Polyfill SkipLast
    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> enumerable, int count)
    {
        if (enumerable is null)
        {
            throw new ArgumentNullException(nameof(enumerable));
        }

        var list = new List<T>(enumerable);
        list.RemoveRange(list.Count - count, count);
        return list;
    }
}
#endif
