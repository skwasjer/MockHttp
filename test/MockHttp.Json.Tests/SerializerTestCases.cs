using System.Collections;

namespace MockHttp.Json;

public class SerializerTestCases : IEnumerable<object?[]>
{
    public IEnumerator<object?[]> GetEnumerator()
    {
        // Null
        yield return [null, "null"];

        // Bool
        yield return [true, "true"];
        yield return [false, "false"];

        // Text
        yield return [string.Empty, "\"\""];
        yield return ["some text", "\"some text\""];

        // Numbers
        yield return [123, "123"];
        yield return [123.456D, "123.456"];
        yield return [-45678913L, "-45678913"];

        // Array with mixed types
        yield return
        [
            new[]
            {
                1, 3, 2
            },
            "[1,3,2]"
        ];
        yield return
        [
            new object[]
            {
                1, 123.456D, "text", true, "3"
            },
            "[1,123.456,\"text\",true,\"3\"]"
        ];

        // Dates
        yield return
        [
            new DateTimeOffset(
                2019,
                07,
                26,
                12,
                34,
                06,
                012,
                TimeSpan.FromHours(2)
            ),
            "\"2019-07-26T12:34:06.012+02:00\""
        ];

        // Complex
        yield return
        [
            new
            {
                value = 1,
                complex = new
                {
                    other = true,
                    arr = new object[]
                    {
                        1, "text", true, "3"
                    }
                }
            },
            "{\"value\":1,\"complex\":{\"other\":true,\"arr\":[1,\"text\",true,\"3\"]}}"
        ];
        yield return
        [
            new Dictionary<string, object>
            {
                {
                    "first", false
                },
                {
                    "second", "2nd"
                }
            },
            "{\"first\":false,\"second\":\"2nd\"}"
        ];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
