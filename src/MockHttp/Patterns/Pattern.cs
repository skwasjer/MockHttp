using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace MockHttp.Patterns;

/// <summary>
/// A pattern that matches a string.
/// </summary>
internal readonly record struct Pattern : IPattern
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

            // When pattern and value are null, effectively this is an Empty pattern so match null or empty strings.
            return _value is null
                ? string.IsNullOrEmpty
                : throw new InvalidOperationException("Pattern is in invalid state.");
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
    /// A pattern that always matches (anything).
    /// </summary>
    public static Pattern Any { get; } = new()
    {
        Value = nameof(Any),
        IsMatch = _ => true
    };

    /// <summary>
    /// A pattern that is empty and matches empty/null strings.
    /// </summary>
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public static Pattern Empty { get; }

    /// <summary>
    /// Returns a pattern that matches the exact <paramref name="value" />.
    /// </summary>
    /// <param name="value">The exact value the pattern matches.</param>
    /// <returns>A new pattern that matches only when the value is an exact match.</returns>
    public static Pattern Exactly(string value)
    {
        return new Pattern
        {
            Value = value,
            IsMatch = input => StringComparer.Ordinal.Equals(value, input)
        };
    }

    /// <summary>
    /// Returns a pattern that allows wildcards '*' to match any arbitrary characters.
    /// </summary>
    /// <param name="pattern">The pattern with wildcard(s) '*' to match any arbitrary characters.</param>
    /// <returns>A new pattern with wildcard support.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pattern" /> is <see langword="null" />.</exception>
    public static Pattern Wildcard(string pattern)
    {
        var wildcardPattern = WildcardPattern.Create(pattern);
        return new Pattern
        {
            Value = wildcardPattern.Value,
            IsMatch = wildcardPattern.IsMatch
        };
    }

    /// <summary>
    /// Returns a pattern that determines a match by executing a regular expression.
    /// </summary>
    /// <param name="pattern">The regular expression to match.</param>
    /// <returns>A new pattern based on a regular expression.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pattern" /> is <see langword="null" />.</exception>
    public static Pattern Regex(
#if NET8_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.Regex)]
#endif
        string pattern
    )
    {
        return Regex(
            new Regex(
                pattern,
                RegexOptions.CultureInvariant | RegexOptions.Singleline,
                TimeSpan.FromSeconds(5)
            )
        );
    }

    /// <summary>
    /// Returns a pattern that determines a match by executing a regular expression.
    /// </summary>
    /// <param name="pattern">The regular expression to match.</param>
    /// <returns>A new pattern based on a regular expression.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pattern" /> is <see langword="null" />.</exception>
    public static Pattern Regex(Regex pattern)
    {
        if (pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern));
        }

        return new Pattern
        {
            Value = pattern.ToString(),
            IsMatch = pattern.IsMatch
        };
    }

    /// <summary>
    /// Returns a pattern that determines a match by invoking the specified <paramref name="expression" />.
    /// </summary>
    /// <param name="expression">The expression that is invoked to determine a match.</param>
    /// <returns>A new pattern based on an expression.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="expression" /> is <see langword="null" />.</exception>
    public static Pattern Expression(Expression<Func<string, bool>> expression)
    {
        if (expression is null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        return new Pattern
        {
            Value = expression.ToString(),
            IsMatch = expression.Compile().Invoke
        };
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA2225
    public static implicit operator Pattern(string value)
    {
        return Exactly(value);
    }

    public static Pattern operator !(Pattern pattern)
    {
        return new Pattern
        {
            Value = $"!= {pattern.Value}",
            IsMatch = s => !pattern.IsMatch(s)
        };
    }

    public static Pattern operator &(Pattern left, Pattern right)
    {
        return new Pattern
        {
            Value = $"({left} & {right})",
            IsMatch = s => left.IsMatch(s) && right.IsMatch(s)
        };
    }

    public static Pattern operator |(Pattern left, Pattern right)
    {
        return new Pattern
        {
            Value = $"({left} | {right})",
            IsMatch = s => left.IsMatch(s) || right.IsMatch(s)
        };
    }

    public static Pattern operator ^(Pattern left, Pattern right)
    {
        return new Pattern
        {
            Value = $"({left} ^ {right})",
            IsMatch = s => left.IsMatch(s) ^ right.IsMatch(s)
        };
    }

    [ExcludeFromCodeCoverage]
    // ReSharper disable once UnusedParameter.Global
    public static bool operator true(Pattern _)
    {
        return false;
    }

    [ExcludeFromCodeCoverage]
    // ReSharper disable once UnusedParameter.Global
    public static bool operator false(Pattern _)
    {
        return false;
    }
#pragma warning restore CA2225
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
