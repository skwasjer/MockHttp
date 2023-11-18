using MockHttp.Responses;

namespace MockHttp.Matchers;

public class NotMatcherTests
{
    private readonly IAsyncHttpRequestMatcher _innerMatcherMock;
    private readonly NotMatcher _sut;

    public NotMatcherTests()
    {
        _innerMatcherMock = Substitute.For<IAsyncHttpRequestMatcher>();
        _sut = new NotMatcher(_innerMatcherMock);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Given_inner_matcher_when_matching_it_should_return_inverse(bool innerMatchResult)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "http://0.0.0.0/url");
        _innerMatcherMock
            .IsMatchAsync(Arg.Any<MockHttpRequestContext>())
            .Returns(Task.FromResult(innerMatchResult));

        // Act & assert
        (await _sut.IsMatchAsync(new MockHttpRequestContext(request)))
            .Should()
            .Be(!innerMatchResult);
        await _innerMatcherMock.Received(1).IsMatchAsync(Arg.Any<MockHttpRequestContext>());
    }

    [Fact]
    public void Given_null_inner_matcher_when_creating_matcher_should_throw()
    {
        IAsyncHttpRequestMatcher? matcher = null;

        // Act
        Func<NotMatcher> act = () => new NotMatcher(matcher!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(matcher));
    }

    [Fact]
    public void When_formatting_should_return_human_readable_representation()
    {
        const string innerMatcherText = "Type: text";
        const string expectedText = $"Not {innerMatcherText}";
        var sut = new NotMatcher(new FakeToStringTestMatcher(innerMatcherText));

        // Act
        string displayText = sut.ToString();

        // Assert
        displayText.Should().Be(expectedText);
    }

    [Fact]
    public async Task Given_null_context_when_matching_it_should_throw()
    {
        MockHttpRequestContext? requestContext = null;

        // Act
        Func<Task> act = () => _sut.IsMatchAsync(requestContext!);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentNullException>()
            .WithParameterName(nameof(requestContext));
    }
}
