using MockHttp.Json.SystemTextJson;

namespace MockHttp.Json;

internal static class Defaults
{
    public static readonly IJsonAdapter Adapter = new SystemTextJsonAdapter();
}
