using MockHttp.Responses;

namespace MockHttp.Matchers;

internal sealed class BoolMatcher : HttpRequestMatcher
{
    private readonly bool _shouldMatch;

    public BoolMatcher(bool shouldMatch)
    {
        _shouldMatch = shouldMatch;
    }

    public override bool IsMatch(MockHttpRequestContext requestContext)
    {
        return _shouldMatch;
    }

    public override string ToString()
    {
        throw new NotImplementedException();
    }
}
