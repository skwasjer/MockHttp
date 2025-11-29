using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace MockHttp.Matchers;

/// <summary>
/// A string matcher encapsulating a delegate and 'pretty' name for debug/display needs when reporting errors to the user.
/// </summary>
internal readonly record struct Matches : IStringMatcher
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string _value;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Func<string, bool> _isMatch;

    /// <inheritdoc />
    public required string Value
    {
        get
        {
            return _value ?? nameof(Empty);
        }
        [MemberNotNull(nameof(_value))]
        init
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <inheritdoc />
    public required Func<string, bool> IsMatch
    {
        get
        {
            if (_isMatch is not null)
            {
                return _isMatch;
            }

            // When matcher delegate and value are null, effectively this is an Empty matcher so match null or empty strings.
            return _value is null
                ? string.IsNullOrEmpty
                : throw new InvalidOperationException($"{nameof(Matches)} is in invalid state.");
        }
        [MemberNotNull(nameof(_isMatch))]
        init
        {
            _isMatch = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// A string matcher that always matches (anything).
    /// </summary>
    public static Matches Any { get; } = new()
    {
        Value = nameof(Any),
        IsMatch = _ => true
    };

    /// <summary>
    /// A string matcher that is empty and matches empty/null strings.
    /// </summary>
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public static Matches Empty { get; }

    /// <summary>
    /// Returns a string matcher that matches the exact <paramref name="value" />.
    /// </summary>
    /// <param name="value">The exact value to match.</param>
    /// <returns>A new string matcher that matches only when the value is an exact match.</returns>
    public static Matches Exactly(string value)
    {
        return new Matches
        {
            Value = value,
            IsMatch = input => StringComparer.Ordinal.Equals(value, input)
        };
    }

    /// <summary>
    /// Returns a string matcher that uses wildcard(s) '*' to match any arbitrary characters.
    /// </summary>
    /// <param name="value">The value with wildcard(s) '*' to match any arbitrary characters.</param>
    /// <returns>A new string matcher that uses wildcard(s).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value" /> is <see langword="null" />.</exception>
    public static Matches Wildcard(string value)
    {
        var wildcardStringMatcher = WildcardStringMatcher.Create(value);
        return new Matches
        {
            Value = wildcardStringMatcher.Value,
            IsMatch = wildcardStringMatcher.IsMatch
        };
    }

    /// <summary>
    /// Returns a string matcher that determines a match by executing a regular expression.
    /// </summary>
    /// <param name="regex">The regular expression to match.</param>
    /// <returns>A new string matcher based on a regular expression.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="regex" /> is <see langword="null" />.</exception>
    public static Matches Regex(
#if NET8_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.Regex)]
#endif
        string regex
    )
    {
        if (regex is null)
        {
            throw new ArgumentNullException(nameof(regex));
        }

        return Regex(
            new Regex(
                regex,
                RegexOptions.CultureInvariant | RegexOptions.Singleline,
                TimeSpan.FromSeconds(5)
            )
        );
    }

    /// <summary>
    /// Returns a string matcher that determines a match by executing a regular expression.
    /// </summary>
    /// <param name="regex">The regular expression to match.</param>
    /// <returns>A new string matcher based on a regular expression.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="regex" /> is <see langword="null" />.</exception>
    public static Matches Regex(Regex regex)
    {
        if (regex is null)
        {
            throw new ArgumentNullException(nameof(regex));
        }

        return new Matches
        {
            Value = regex.ToString(),
            IsMatch = regex.IsMatch
        };
    }

    /// <summary>
    /// Returns a string matcher that determines a match by invoking the specified <paramref name="expression" />.
    /// </summary>
    /// <param name="expression">The expression that is invoked to determine a match.</param>
    /// <returns>A new string matcher based on an expression.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="expression" /> is <see langword="null" />.</exception>
    public static Matches Expression(Expression<Func<string, bool>> expression)
    {
        if (expression is null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        return new Matches
        {
            Value = expression.ToString(),
            IsMatch = expression.Compile().Invoke
        };
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA2225
    public static implicit operator Matches(string value)
    {
        return Exactly(value);
    }

    public static Matches operator !(Matches matches)
    {
        return new Matches
        {
            Value = $"!= {matches.Value}",
            IsMatch = s => !matches.IsMatch(s)
        };
    }

    public static Matches operator &(Matches left, Matches right)
    {
        return new Matches
        {
            Value = $"({left} & {right})",
            IsMatch = s => left.IsMatch(s) && right.IsMatch(s)
        };
    }

    public static Matches operator |(Matches left, Matches right)
    {
        return new Matches
        {
            Value = $"({left} | {right})",
            IsMatch = s => left.IsMatch(s) || right.IsMatch(s)
        };
    }

    public static Matches operator ^(Matches left, Matches right)
    {
        return new Matches
        {
            Value = $"({left} ^ {right})",
            IsMatch = s => left.IsMatch(s) ^ right.IsMatch(s)
        };
    }

    [ExcludeFromCodeCoverage]
    // ReSharper disable once UnusedParameter.Global
    public static bool operator true(Matches _)
    {
        return false;
    }

    [ExcludeFromCodeCoverage]
    // ReSharper disable once UnusedParameter.Global
    public static bool operator false(Matches _)
    {
        return false;
    }
#pragma warning restore CA2225
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
