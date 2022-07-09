namespace MockHttp.Json;

internal static class DeprecationWarnings
{
    public const string RespondsExtensions = "Obsolete, will be removed in next major release. Use the .Respond(with => with.JsonBody(..)) extension methods.";
    public const string JsonContent = "Obsolete, will be removed in next major release. Use the .When(matching => matching.JsonBody(..)) extension methods.";
}
