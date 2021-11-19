using System.Collections;

namespace MockHttp.Json;

public class JsonMatchTestCases : IEnumerable<object?[]>
{
    public IEnumerator<object?[]> GetEnumerator()
    {
        // Null
        yield return new object[] { string.Empty, string.Empty };
        yield return new object[] { string.Empty, "null" };
        yield return new object?[] { string.Empty, null };
        yield return new object[] { "null", "null" };
        yield return new object[] { "null", string.Empty };
        yield return new object?[] { "null", null };
        yield return new object?[] { null, "null" };
        yield return new object?[] { null, null };
        yield return new object?[] { null, string.Empty };

        // Bool
        yield return new object[] { "true", "true" };
        yield return new object[] { "false", "false" };

        // Text
        yield return new object[] { "\"\"", "\"\"" };
        yield return new object[] { "\"some text\"", "\"some text\"" };

        // Numbers
        yield return new object[] { "123", "123" };
        yield return new object[] { "123.456", "123.456" };
        yield return new object[] { "-45678913", "-45678913" };

        // Array with mixed types
        yield return new object[] { "[1,3,2]", "[1,3,2]" };
        yield return new object[] { "[1,2.1,\"text\",true,\"3\"]", "[1,2.1,\"text\",true,\"3\"]" };

        // Complex
        yield return new object[] { "{\"value\":1,\"complex\":{\"other\":true}}", "{\"value\":1,\"complex\":{\"other\":true}}" };

        // Complex with mixed property order.
        yield return new object[] { "{\"first\":\"order\",\"second\":false}", "{\"second\":false,\"first\":\"order\"}" };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
