namespace MockHttp.Matchers.Patterns;

internal abstract class PatternMatcher
{
    /// <summary>
    /// Tests if the specified <paramref name="value" /> matches the pattern.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns>Returns true if the value matches the pattern; otherwise returns false.</returns>
    public abstract bool IsMatch(string value);

    /// <inheritdoc />
    public abstract override string ToString();
}
