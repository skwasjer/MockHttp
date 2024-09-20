using MockHttp.Matchers;

namespace MockHttp.Request;

internal sealed class InvertRequestMatching : RequestMatching
{
    private readonly RequestMatching _requestMatching;

    public InvertRequestMatching(RequestMatching requestMatching)
    {
        _requestMatching = requestMatching;
    }

    protected internal override RequestMatching RegisterMatcher(IAsyncHttpRequestMatcher matcher)
    {
        return _requestMatching.RegisterMatcher(new NotMatcher(matcher));
    }
}
