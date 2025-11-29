using System.Text;

namespace MockHttp.Matchers;

internal readonly record struct WildcardStringMatcher : IStringMatcher
{
    private static readonly char[] WildcardSplit = ['*'];
    private static readonly char[] SpecialRegexChars = ['.', '+', '*', '?', '^', '$', '(', ')', '[', ']', '{', '}', '|', '\\'];

    internal Matches RegexMatches { get; private init; }

    /// <inheritdoc />
    public required string Value { get; internal init; }

    /// <inheritdoc />
    public required Func<string, bool> IsMatch { get; internal init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }

    internal static WildcardStringMatcher Create(string value)
    {
        var regexMatches = Matches.Regex(ConvertWildcardToRegex(value));
        return new WildcardStringMatcher
        {
            RegexMatches = regexMatches,
            Value = value,
            IsMatch = regexMatches.IsMatch
        };
    }

    private static string ConvertWildcardToRegex(string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value.Length == 0)
        {
            throw new ArgumentException("The value cannot be empty.", nameof(value));
        }

        var sb = new StringBuilder();
        bool startsWithWildcard = value[0] == '*';
        bool endsWithWildcard = value[value.Length - 1] == '*';
        if (startsWithWildcard)
        {
            value = value.TrimStart('*');
            sb.Append(".*");
        }
        else
        {
            sb.Append('^');
        }

        if (endsWithWildcard)
        {
            if (startsWithWildcard && value.Length == 0)
            {
                return sb.ToString();
            }

            value = value.TrimEnd('*');
        }

        IEnumerable<string> matchGroups = value
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
