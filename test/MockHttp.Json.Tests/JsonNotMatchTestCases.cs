using System.Collections;

namespace MockHttp.Json;

public class JsonNotMatchTestCases : IEnumerable<object?[]>
{
    public IEnumerator<object?[]> GetEnumerator()
    {
        // We want to test these cases for both left and right arguments of the equality operator.
        return new TestCases()
            .Union(
                new TestCases()
                    .Select(tc => ((IEnumerable<object?>)tc)
                        .Reverse()
                        .ToArray()
                    )
            )
            .Distinct()
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class TestCases : IEnumerable<object?[]>
    {
        public IEnumerator<object?[]> GetEnumerator()
        {
            // Null/empty short-circuit
            yield return ["123", null];
            yield return ["123", string.Empty];

            // Bool
            yield return ["true", "false"];
            yield return ["true", "\"true\""];

            // Text
            yield return ["\"some text\"", "\"other text\""];

            // Numbers
            yield return ["123", "124"];
            yield return ["123", "\"123\""];
            yield return ["123.456", "123.4561"];

            // Array + with mixed types
            yield return ["[1, 2, 3, 4]", "[1, 2, 3]"];
            yield return ["[1,2.1,\"3\"]", "[1,2.1,3]"];
            yield return ["[1, 2, 3, 4]", "[1, 2, 4, 3]"];

            // Complex

            // Other value
            yield return ["{\"value\":1,\"complex\":{\"other\":true}}", "{\"value\":2,\"complex\":{\"other\":true}}"];

            // Nested other value
            yield return ["{\"value\":1,\"complex\":{\"other\":true}}", "{\"value\":1,\"complex\":{\"other\":false}}"];

            // Nested with different number of properties
            yield return
            [
                "{\"value\":1,\"complex\":{\"other\":true}}", "{\"value\":1,\"complex\":{\"other\":true,\"and-another\":\"text\"}}"
            ];

            // Properties with other value types.
            yield return ["{\"value\":1}", "{\"value\":true}"];
            yield return ["{\"value\":1}", "{\"value\":\"1\"}"];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
