using System.Collections;

namespace MockHttp.Json;

public class JsonNotMatchTestCases : IEnumerable<object?[]>
{
    public IEnumerator<object?[]> GetEnumerator()
    {
        // We want to test these cases for both left and right arguments of the equality operator.
        return new TestCases()
            .Union(new TestCases()
                .Select(tc => tc
                    .Reverse()
                    .ToArray())
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
            yield return new object?[] { "123", null };
            yield return new object?[] { "123", string.Empty };

            // Bool
            yield return new object[] { "true", "false" };

            // Text
            yield return new object[] { "\"some text\"", "\"other text\"" };

            // Numbers
            yield return new object[] { "123", "124" };
            yield return new object[] { "123", "\"123\"" };
            yield return new object[] { "123.456", "123.4561" };

            // Array + with mixed types
            yield return new object[] { "[1, 2, 3, 4]", "[1, 2, 3]" };
            yield return new object[] { "[1,2.1,\"3\"]", "[1,2.1,3]" };
            yield return new object[] { "[1, 2, 3, 4]", "[1, 2, 4, 3]" };

            // Complex
            yield return new object[] { "{\"value\":1,\"complex\":{\"other\":true}}", "{\"value\":2,\"complex\":{\"other\":true}}" };
            yield return new object[] { "{\"value\":1,\"complex\":{\"other\":true}}", "{\"value\":1,\"complex\":{\"other\":false}}" };
            yield return new object[] { "{\"value\":1,\"complex\":{\"other\":true}}", "{\"value\":1,\"complex\":{\"other\":true,\"and-another\":\"text\"}}" };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
