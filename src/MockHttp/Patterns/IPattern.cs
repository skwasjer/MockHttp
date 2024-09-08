namespace MockHttp.Patterns;

internal interface IPattern
{
    string Value { get; }
    Func<string, bool> IsMatch { get; }
}
