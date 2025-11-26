namespace MockHttp.Matchers;

internal sealed class FakeToStringTestMatcher : HttpRequestMatcher
{
    private readonly string _matcherName;

    public FakeToStringTestMatcher(string matcherName)
    {
        _matcherName = matcherName;
    }

    public override bool IsMatch(MockHttpRequestContext requestContext)
    {
        throw new NotImplementedException();
    }

    public override string ToString() => _matcherName;
}
