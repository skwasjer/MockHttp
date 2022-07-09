namespace MockHttp;

internal static class DeprecationWarnings
{
    public const string RespondsExtensions = "Obsolete, will be removed in next major release. Use the fluent .Respond(with => with.StatusCode(..).Body(..)) fluent API.";
    public const string RespondsTimeoutExtensions = "Obsolete, will be removed in next major release. Use the fluent .Respond(with => with.Timeout(..)) fluent API.";
    public const string RequestContent = "Obsolete, will be removed in next major release. Use the fluent .When(matching => matching.Body(..)) fluent API.";
    public const string RequestWithoutContent = "Obsolete, will be removed in next major release. Use the fluent .When(matching => matching.WithoutBody()) fluent API.";
    public const string RequestPartialContent = "Obsolete, will be removed in next major release. Use the fluent .When(matching => matching.PartialBody(..)) fluent API.";
    public const string RespondsResult = "Obsolete, will be removed in next major release.";
}
