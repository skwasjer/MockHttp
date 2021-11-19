using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MockHttp.Json.Newtonsoft;

internal class NewtonsoftEqualityComparer : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        // We consider any combination of null, JSON 'null', or JSON empty
        // documents as equal.
        if (string.IsNullOrEmpty(x))
        {
            x = "null";
        }
        if (string.IsNullOrEmpty(y))
        {
            y = "null";
        }

        JToken? xToken = JsonConvert.DeserializeObject<JToken>(x);
        JToken? yToken = JsonConvert.DeserializeObject<JToken>(y);
        return JToken.DeepEquals(xToken, yToken);
    }

    public int GetHashCode(string obj)
    {
        return obj.GetHashCode();
    }
}
