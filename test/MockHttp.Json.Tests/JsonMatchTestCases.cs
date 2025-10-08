using System.Collections;

namespace MockHttp.Json;

public class JsonMatchTestCases : IEnumerable<object?[]>
{
    public IEnumerator<object?[]> GetEnumerator()
    {
        // Null
        yield return [string.Empty, string.Empty];
        yield return [string.Empty, "null"];
        yield return [string.Empty, null];
        yield return ["null", "null"];
        yield return ["null", string.Empty];
        yield return ["null", null];
        yield return [null, "null"];
        yield return [null, null];
        yield return [null, string.Empty];

        // Bool
        yield return ["true", "true"];
        yield return ["false", "false"];

        // Text
        yield return ["\"\"", "\"\""];
        yield return ["\"some text\"", "\"some text\""];

        // Numbers
        yield return ["123", "123"];
        yield return ["123.456", "123.456"];
        yield return ["-45678913", "-45678913"];

        // Array with mixed types
        yield return ["[1,3,2]", "[1,3,2]"];
        yield return ["[1,2.1,\"text\",true,\"3\"]", "[1,2.1,\"text\",true,\"3\"]"];

        // Complex
        yield return ["{\"value\":1,\"complex\":{\"other\":true}}", "{\"value\":1,\"complex\":{\"other\":true}}"];

        // Complex with mixed property order.
        yield return ["{\"first\":\"order\",\"second\":false}", "{\"second\":false,\"first\":\"order\"}"];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
