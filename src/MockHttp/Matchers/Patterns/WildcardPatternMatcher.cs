﻿using System.Text;

namespace MockHttp.Matchers.Patterns;

internal sealed class WildcardPatternMatcher : RegexPatternMatcher
{
    private static readonly char[] SpecialRegexChars = ['.', '+', '*', '?', '^', '$', '(', ')', '[', ']', '{', '}', '|', '\\'];

    private readonly string _pattern;

    public WildcardPatternMatcher(string pattern)
        : base(GetMatchPattern(pattern))
    {
        _pattern = pattern;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _pattern;
    }

    private static string GetMatchPattern(string pattern)
    {
        if (pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern));
        }

        var sb = new StringBuilder();
        bool startsWithWildcard = pattern[0] == '*';
        if (startsWithWildcard)
        {
            pattern = pattern.TrimStart('*');
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
            .Select(s => $"({RegexUriEscape(s)})");

        sb.Append(string.Join(".+", matchGroups));

        sb.Append(endsWithWildcard ? ".*" : "$");

        return sb.ToString();
    }

    private static string RegexUriEscape(string s)
    {
        var sb = new StringBuilder();
        foreach (char ch in s)
        {
            if (SpecialRegexChars.Contains(ch))
            {
                sb.Append('\\');
            }

            sb.Append(ch);
        }

        return sb.ToString();
    }
}
