using System.Collections;

namespace MockHttp.Json;

public class SerializerTestCases : IEnumerable<object?[]>
{
    public IEnumerator<object?[]> GetEnumerator()
    {
        // Null
        yield return new object?[] { null, "null" };

        // Bool
        yield return new object[] { true, "true" };
        yield return new object[] { false, "false" };

        // Text
        yield return new object[] { string.Empty, "\"\"" };
        yield return new object[] { "some text", "\"some text\"" };

        // Numbers
        yield return new object[] { 123, "123" };
        yield return new object[] { 123.456D, "123.456" };
        yield return new object[] { -45678913L, "-45678913" };

        // Array with mixed types
        yield return new object[] { new[] { 1, 3, 2 }, "[1,3,2]" };
        yield return new object[] { new object[] { 1, 123.456D, "text", true, "3" }, "[1,123.456,\"text\",true,\"3\"]" };

        // Dates
        yield return new object[]
        {
            new DateTimeOffset(2019, 07, 26, 12, 34, 06, 012, TimeSpan.FromHours(2)),
            "\"2019-07-26T12:34:06.012+02:00\""
        };

        // Complex
        yield return new object[]
        {
            new { value = 1, complex = new { other = true, arr = new object[] { 1, "text", true, "3" } } },
            "{\"value\":1,\"complex\":{\"other\":true,\"arr\":[1,\"text\",true,\"3\"]}}"
        };
        yield return new object[] { new Dictionary<string, object> { { "first", false }, { "second", "2nd" } }, "{\"first\":false,\"second\":\"2nd\"}" };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
