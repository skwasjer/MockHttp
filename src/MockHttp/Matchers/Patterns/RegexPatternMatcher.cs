using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MockHttp.Matchers.Patterns;

internal class RegexPatternMatcher : PatternMatcher
{
    public RegexPatternMatcher
    (
#if NET8_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.Regex)]
#endif
        string regex
    )
        : this(new Regex(regex, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline))
    {
    }

    public RegexPatternMatcher(Regex regex)
    {
        Regex = regex ?? throw new ArgumentNullException(nameof(regex));
    }

    internal Regex Regex { get; }

    /// <inheritdoc />
    public override bool IsMatch(string value)
    {
        return Regex.IsMatch(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Regex.ToString();
    }
}
