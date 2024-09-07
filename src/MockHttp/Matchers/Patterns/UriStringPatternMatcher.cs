namespace MockHttp.Matchers.Patterns;

internal class UriStringPatternMatcher : IPatternMatcher<Uri>
{
    private readonly Func<Uri, string> _selector;
    private readonly IPatternMatcher<string> _stringPatternMatcher;

    public UriStringPatternMatcher(Func<Uri, string> selector, IPatternMatcher<string> stringPatternMatcher)
    {
        _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        _stringPatternMatcher = stringPatternMatcher ?? throw new ArgumentNullException(nameof(stringPatternMatcher));
    }

    public bool IsMatch(Uri value)
    {
        string v = _selector(value);
        return _stringPatternMatcher.IsMatch(v);
    }
}
