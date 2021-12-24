using MockHttp.Json.Newtonsoft;

namespace MockHttp.Json;

internal static class Defaults
{
    public static readonly IJsonAdapter Adapter = new NewtonsoftAdapter();
}
