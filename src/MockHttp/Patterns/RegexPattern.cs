using System.Text.RegularExpressions;

namespace MockHttp.Patterns;

internal readonly record struct RegexPattern : IPattern
{
    public required string Value { get; internal init; }
    public required Func<string, bool> IsMatch { get; internal init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }

    public static implicit operator Pattern(RegexPattern pattern)
    {
        return new Pattern
        {
            Value = pattern.Value,
            IsMatch = pattern.IsMatch
        };
    }

    public static RegexPattern Create
    (
#if NET8_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.StringSyntax(System.Diagnostics.CodeAnalysis.StringSyntaxAttribute.Regex)]
#endif
        string pattern
    )
    {
        return Create(new Regex(
            pattern,
            RegexOptions.CultureInvariant | RegexOptions.Singleline,
            TimeSpan.FromSeconds(5))
        );
    }

    public static RegexPattern Create(Regex pattern)
    {
        if (pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern));
        }

        return new RegexPattern
        {
            Value = pattern.ToString(),
            IsMatch = pattern.IsMatch
        };
    }
}
