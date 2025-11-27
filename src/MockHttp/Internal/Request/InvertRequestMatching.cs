using MockHttp.Language;
using MockHttp.Matchers;

namespace MockHttp.Request;

internal sealed class InvertRequestMatching : RequestMatching
{
    private readonly RequestMatching _requestMatching;

    public InvertRequestMatching(RequestMatching requestMatching)
    {
        _requestMatching = requestMatching;
    }

    public override IRequestMatching Add(IAsyncHttpRequestMatcher matcher)
    {
        return _requestMatching.Add(new NotMatcher(matcher));
    }
}
