using System.Net;
using System.Text;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Language;
using MockHttp.Language.Flow;
using MockHttp.Responses;
using Moq;
using Xunit;

namespace MockHttp.Extensions;

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
            _sut.Respond((ctx, _) => new HttpResponseMessage { Headers = { { "query", ctx.RequestUri!.Query } } });
            HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

            // Assert
            actualResponse.Should().HaveHeader("query", request.RequestUri!.Query);
        }
    }

    public class WithStream : IRespondsExtensionsTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task When_responding_with_stream_it_should_return_response(bool isSeekable)
        {
            using var ms = new CanSeekMemoryStream(Encoding.UTF8.GetBytes("content"), isSeekable);
            var request = new HttpRequestMessage();
            var expectedContent = new ByteArrayContent(Encoding.UTF8.GetBytes("content"));

            // Act
            _sut.Respond(with => with.Body(ms));
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

        [Fact]
        public void When_responding_with_null_stream_it_should_throw()
        {
            Stream? streamContent = null;

            // Act
            Action act = () => _sut.Respond(with => with.Body(streamContent!));

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName(nameof(streamContent));
        }

        [Fact]
        public void When_responding_with_not_readable_stream_it_should_throw()
        {
            var streamMock = new Mock<Stream>();
            streamMock.Setup(m => m.CanRead).Returns(false);
            Stream? streamContent = streamMock.Object;

            // Act
            Action act = () => _sut.Respond(with => with.Body(streamContent));

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName(nameof(streamContent))
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
            _sut.Respond(with => with.Body(httpContent));
            HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

            // Assert
            await actualResponse.Should()
                .HaveStatusCode(HttpStatusCode.OK)
                .And.HaveContentAsync(expectedContent);
        }

        [Fact]
        public void When_responding_with_null_httpContent_it_should_throw()
        {
            HttpContent? httpContent = null;

            // Act
            Action act = () => _sut.Respond(with => with.Body(httpContent!));

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName(nameof(httpContent));
        }

        [Fact]
        public async Task When_responding_with_string_content_it_should_return_response()
        {
            var request = new HttpRequestMessage();

            // Act
            _sut.Respond(with => with.Body("content"));
            HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

            // Assert
            await actualResponse.Should()
                .HaveStatusCode(HttpStatusCode.OK)
                .And.HaveContentAsync("content");
        }

        [Fact]
        public void When_responding_with_null_string_content_it_should_throw()
        {
            string? textContent = null;

            // Act
            Action act = () => _sut.Respond(with => with.Body(textContent!));

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName(nameof(textContent));
        }

        [Fact]
        public async Task When_responding_with_string_content_and_media_type_it_should_return_response()
        {
            const string mediaType = "my/mediatype";
            const string expectedMediaType = "my/mediatype; charset=utf-8";
            var request = new HttpRequestMessage();

            // Act
            _sut.Respond(with => with
                .Body("content")
                .ContentType(mediaType, Encoding.UTF8)
            );
            HttpResponseMessage actualResponse = await _httpCall.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

            // Assert
            actualResponse.Should()
                .HaveStatusCode(HttpStatusCode.OK)
                .And.HaveContentType(expectedMediaType);
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

            DelegateTestCase[] testCases =
            {
                DelegateTestCase.Create(
                    IRespondsExtensions.Respond,
                    responds,
                    () => new HttpResponseMessage()),
                DelegateTestCase.Create<IResponds<IResponseResult>, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>, IResponseResult>(
                    IRespondsExtensions.Respond,
                    responds,
                    (_, _) => new HttpResponseMessage()),
                DelegateTestCase.Create(
                    IRespondsExtensions.Respond,
                    responds,
                    (Action<MockHttpRequestContext, CancellationToken, IResponseBuilder>)((_, _, _) => { })),
                DelegateTestCase.Create(
                    IRespondsExtensions.RespondUsing<FakeResponseStrategy, IResponseResult>,
                    responds),
                DelegateTestCase.Create(
                    IRespondsExtensions.Respond,
                    responds,
                    (Action<IResponseBuilder>)(_ => { })),
                DelegateTestCase.Create(
                    IRespondsExtensions.Respond,
                    responds,
                    (Action<MockHttpRequestContext, IResponseBuilder>)((_, _) => { }))
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
#nullable restore
