namespace MockHttp.Matchers.Patterns;

internal interface IPatternMatcher<in T>
{
    /// <summary>
    /// Tests if the specified <paramref name="value" /> matches the pattern.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns>Returns true if the value matches the pattern; otherwise returns false.</returns>
    bool IsMatch(T value);
}
