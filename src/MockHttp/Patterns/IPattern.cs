namespace MockHttp.Patterns;

/// <summary>
/// Describes a pattern that matches a string.
/// </summary>
internal interface IPattern
{
    /// <summary>
    /// Gets the pattern value.
    /// </summary>
    string Value { get; }

    /// <summary>
    /// Gets the pattern match delegate.
    /// </summary>
    Func<string, bool> IsMatch { get; }
}
