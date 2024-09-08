namespace MockHttp.Patterns;

internal readonly record struct Pattern : IPattern
{
    public required string Value { get; internal init; }
    public required Func<string, bool> IsMatch { get; internal init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }

    public static Pattern MatchExactly(string value)
    {
        return new Pattern
        {
            Value = value,
            IsMatch = input => StringComparer.Ordinal.Equals(value, input)
        };
    }
}
