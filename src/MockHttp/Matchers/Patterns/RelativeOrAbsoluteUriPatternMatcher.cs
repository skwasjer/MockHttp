using System.Diagnostics;
using MockHttp.Http;

namespace MockHttp.Matchers.Patterns;

[DebuggerDisplay($"{{{nameof(_originalUri)}}}")]
internal sealed class RelativeOrAbsoluteUriPatternMatcher : IPatternMatcher<Uri>
{
    private readonly Uri _originalUri;
    private readonly Uri _uri;

    public RelativeOrAbsoluteUriPatternMatcher(Uri uri)
    {
        _originalUri = uri;
        _uri = uri.EnsureIsRooted();
    }

    public bool IsMatch(Uri value)
    {
        return IsAbsoluteUriMatch(value) || IsRelativeUriMatch(value);
    }

    private bool IsAbsoluteUriMatch(Uri uri)
    {
        return _uri.IsAbsoluteUri && uri.Equals(_uri);
    }

    private bool IsRelativeUriMatch(Uri uri)
    {
        return !_uri.IsAbsoluteUri
         && uri.IsBaseOf(_uri)
         && uri.ToString().EndsWith(_uri.ToString(), StringComparison.Ordinal);
    }
}
