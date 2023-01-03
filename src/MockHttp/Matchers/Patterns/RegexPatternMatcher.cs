using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MockHttp.Matchers.Patterns;

internal class RegexPatternMatcher : PatternMatcher
{
    private readonly Regex _regex;

    public RegexPatternMatcher
    (
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.Regex)]
#endif
        string regex
    )
        : this(new Regex(regex, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline))
    {
    }

    public RegexPatternMatcher(Regex regex)
    {
        _regex = regex ?? throw new ArgumentNullException(nameof(regex));
    }

    /// <inheritdoc />
    public override bool IsMatch(string value)
    {
        return _regex.IsMatch(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _regex.ToString();
    }
}
