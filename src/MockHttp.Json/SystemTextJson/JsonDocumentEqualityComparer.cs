using System.Text.Json;

namespace MockHttp.Json.SystemTextJson;

internal sealed class JsonDocumentEqualityComparer
    : IEqualityComparer<JsonDocument>
{
    private readonly JsonSerializerOptions? _options;

    internal JsonDocumentEqualityComparer(JsonSerializerOptions? options)
    {
        _options = options;
    }

    public bool Equals(JsonDocument? x, JsonDocument? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        // Clone the options to use during this equal comparison (thread safety).
        JsonSerializerOptions opts = _options is null
            ? new JsonSerializerOptions()
            : new JsonSerializerOptions(_options);

        return ElemEquals(x.RootElement, y.RootElement, opts);
    }

    public int GetHashCode(JsonDocument obj)
    {
        return obj.GetHashCode();
    }

    private static bool ElemEquals(JsonElement left, JsonElement right, JsonSerializerOptions serializerOptions)
    {
        if (left.ValueKind != right.ValueKind)
        {
            return false;
        }

        return left.ValueKind switch
        {
            JsonValueKind.Undefined => true,
            JsonValueKind.Null => true,
            JsonValueKind.False => true,
            JsonValueKind.True => true,
            JsonValueKind.String => left.ValueEquals(right.GetString()),
            JsonValueKind.Number => string.Equals(left.GetRawText(), right.GetRawText(), StringComparison.Ordinal),
            JsonValueKind.Array => ArrayEquals(left, right, serializerOptions),
            JsonValueKind.Object => ObjectEquals(left, right, serializerOptions),
            _ => false
        };
    }

    private static bool ObjectEquals(JsonElement left, JsonElement right, JsonSerializerOptions serializerOptions)
    {
        StringComparison comparison = serializerOptions.PropertyNameCaseInsensitive
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        var xProps = left
            .EnumerateObject()
            .ToList();
        var yProps = right
            .EnumerateObject()
            .ToList();

        // We don't care about order of properties currently.
        // If they are in different order, we still consider the document the same.
        return xProps.Count == yProps.Count
         && xProps
                .All(leftProp => yProps
                    .Where(p => leftProp.Name.Equals(p.Name, comparison))
                    .Any(rightProp => ElemEquals(leftProp.Value, rightProp.Value, serializerOptions))
                );
    }

    private static bool ArrayEquals(JsonElement left, JsonElement right, JsonSerializerOptions serializerOptions)
    {
        int count = left.GetArrayLength();
        if (count != right.GetArrayLength())
        {
            return false;
        }

        // Must have exact array order.
        for (int i = 0; i < count; i++)
        {
            if (!ElemEquals(left[i], right[i], serializerOptions))
            {
                return false;
            }
        }

        return true;
    }
}
