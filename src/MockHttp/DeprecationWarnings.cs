namespace MockHttp;

internal static class DeprecationWarnings
{
    public const string RespondsExtensions = "Obsolete, will be removed in next major release. Use the fluent .Respond(with => with.StatusCode(..).Body(..)) fluent API.";
    public const string RespondsTimeoutExtensions = "Obsolete, will be removed in next major release. Use the fluent .Respond(with => with.Timeout(..)) fluent API.";
}
