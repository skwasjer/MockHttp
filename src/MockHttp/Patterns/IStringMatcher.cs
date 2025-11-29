namespace MockHttp.Patterns;

/// <summary>
/// Describes a string matcher.
/// </summary>
internal interface IStringMatcher
{
    /// <summary>
    /// Gets the matcher display value/name.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the matcher delegate.
    /// </summary>
    public Func<string, bool> IsMatch { get; }
}
