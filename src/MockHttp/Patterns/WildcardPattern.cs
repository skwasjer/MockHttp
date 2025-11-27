using System.Text;

namespace MockHttp.Patterns;

internal readonly record struct WildcardPattern : IPattern
{
    private static readonly char[] WildcardSplit = ['*'];
    private static readonly char[] SpecialRegexChars = ['.', '+', '*', '?', '^', '$', '(', ')', '[', ']', '{', '}', '|', '\\'];

    internal Pattern RegexPattern { get; private init; }

    /// <inheritdoc />
    public required string Value { get; internal init; }

    /// <inheritdoc />
    public required Func<string, bool> IsMatch { get; internal init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }

    internal static WildcardPattern Create(string pattern)
    {
        var regexPattern = Pattern.Regex(GetMatchPattern(pattern));
        return new WildcardPattern
        {
            RegexPattern = regexPattern,
            Value = pattern,
            IsMatch = regexPattern.IsMatch
        };
    }

    private static string GetMatchPattern(string pattern)
    {
        if (pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern));
        }

        if (pattern.Length == 0)
        {
            throw new ArgumentException("The pattern cannot be empty.", nameof(pattern));
        }

        var sb = new StringBuilder();
        bool startsWithWildcard = pattern[0] == '*';
        bool endsWithWildcard = pattern[pattern.Length - 1] == '*';
        if (startsWithWildcard)
        {
            pattern = pattern.TrimStart('*');
            sb.Append(".*");
        }
        else
        {
            sb.Append('^');
        }

        if (endsWithWildcard)
        {
            if (startsWithWildcard && pattern.Length == 0)
            {
                return sb.ToString();
            }

            pattern = pattern.TrimEnd('*');
        }

        IEnumerable<string> matchGroups = pattern
            .Split(WildcardSplit, StringSplitOptions.RemoveEmptyEntries)
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
