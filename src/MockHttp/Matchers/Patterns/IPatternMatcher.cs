namespace MockHttp.Matchers.Patterns;

internal interface IPatternMatcher<in T>
{
    bool IsMatch(T value);
}
