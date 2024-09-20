using MockHttp.Matchers;

namespace MockHttp.Request;

/// <summary>
/// A builder to configure request matchers, accepting all matchers.
/// </summary>
internal sealed class AnyRequestMatching : RequestMatching
{
    protected internal override void ValidateMatcher(IAsyncHttpRequestMatcher matcher)
    {
        // Ignore validation.
    }
}
