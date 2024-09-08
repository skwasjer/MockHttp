using System.Text;

namespace MockHttp.Patterns;

internal readonly record struct WildcardPattern : IPattern
{
    private static readonly char[] SpecialRegexChars = ['.', '+', '*', '?', '^', '$', '(', ')', '[', ']', '{', '}', '|', '\\'];

    internal RegexPattern RegexPattern { get; private init; }
    public required string Value { get; internal init; }
    public required Func<string, bool> IsMatch { get; internal init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }

    public static implicit operator Pattern(WildcardPattern pattern)
    {
        return new Pattern
        {
            Value = pattern.Value,
            IsMatch = pattern.IsMatch
        };
    }

    public static WildcardPattern Create(string pattern)
    {
        if (pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern));
        }

        var regexPattern = RegexPattern.Create(GetMatchPattern(pattern));
        return new WildcardPattern
        {
            RegexPattern = regexPattern,
            Value = pattern,
            IsMatch = regexPattern.IsMatch
        };
    }

    private static string GetMatchPattern(string pattern)
    {
        var sb = new StringBuilder();
        bool startsWithWildcard = pattern[0] == '*';
        if (startsWithWildcard)
        {
            pattern = pattern.Substring(1);
            sb.Append(".*");
        }
        else
        {
            sb.Append('^');
        }

        // ReSharper disable once UseIndexFromEndExpression
        bool endsWithWildcard = pattern.Length > 0 && pattern[pattern.Length - 1] == '*';
        if (endsWithWildcard)
        {
            pattern = pattern.TrimEnd('*');
        }

        IEnumerable<string> matchGroups = pattern
            .Split('*')
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => $"({EscapeSpecialRegexChars(s)})");

        sb.Append(string.Join(".+", matchGroups));

        sb.Append(endsWithWildcard ? ".*" : "$");

        return sb.ToString();
    }

    private static string EscapeSpecialRegexChars(string s)
    {
        bool isStrModified = false;
        var sb = new StringBuilder(Math.Min(s.Length, 128));
        foreach (char ch in s)
        {
            if (SpecialRegexChars.Contains(ch))
            {
                sb.Append('\\');
                isStrModified = true;
            }

            sb.Append(ch);
        }

        return isStrModified ? sb.ToString() : s;
    }
}
