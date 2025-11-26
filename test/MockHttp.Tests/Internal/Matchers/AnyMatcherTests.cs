namespace MockHttp.Matchers;

public class AnyMatcherTests
{
    private readonly List<HttpRequestMatcher> _matchers;
    private readonly AnyMatcher _sut;

    public AnyMatcherTests()
    {
        _matchers = [];
        _sut = new AnyMatcher(_matchers);
    }

    [Theory]
    [InlineData("url1")]
    [InlineData("url2")]
    public async Task Given_request_uri_equals_one_of_the_matchers_when_matching_should_match(string requestUrl)
    {
        _matchers.Add(new BoolMatcher(requestUrl == "url1"));
        _matchers.Add(new BoolMatcher(requestUrl == "url2"));

        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        // Act & assert
        (await _sut.IsMatchAsync(new MockHttpRequestContext(request)))
            .Should()
            .BeTrue("the request url '{0}' should match one of the matchers", requestUrl);
    }

    [Fact]
    public async Task Given_request_uri_matches_none_of_the_matchers_when_matching_should_not_match()
    {
        _matchers.Add(new BoolMatcher(false));
        _matchers.Add(new BoolMatcher(false));

        var request = new HttpRequestMessage(HttpMethod.Get, "http://127.0.0.3/");

        // Act & assert
        (await _sut.IsMatchAsync(new MockHttpRequestContext(request)))
            .Should()
            .BeFalse("the request url should not match any of the matchers");
    }

    [Fact]
    public void Given_null_matchers_when_creating_matcher_should_throw()
    {
        IReadOnlyCollection<IAsyncHttpRequestMatcher>? matchers = null;

        // Act
        Func<AnyMatcher> act = () => new AnyMatcher(matchers!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(matchers));
    }

    [Fact]
    public void When_formatting_should_return_human_readable_representation()
    {
        const string innerMatcherText = "Type: text";
        string expectedText = string.Format(@"Any:{1}{{{1}	{0}1{1}	{0}2{1}}}", innerMatcherText, Environment.NewLine);
        var matcherMock1 = new FakeToStringTestMatcher(innerMatcherText + "1");
        var matcherMock2 = new FakeToStringTestMatcher(innerMatcherText + "2");
        _matchers.Add(matcherMock1);
        _matchers.Add(matcherMock2);

        // Act
        string displayText = _sut.ToString();

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
