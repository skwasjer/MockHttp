using System.Text.Json;

namespace MockHttp.Json.SystemTextJson;

internal sealed class SystemTextJsonEqualityComparer
    : IEqualityComparer<string>
{
    private readonly JsonSerializerOptions? _options;
    private readonly IEqualityComparer<JsonDocument> _documentComparer;

    internal SystemTextJsonEqualityComparer(JsonSerializerOptions? options)
        : this(new JsonDocumentEqualityComparer(options), options)
    {
    }

    internal SystemTextJsonEqualityComparer(IEqualityComparer<JsonDocument> documentComparer, JsonSerializerOptions? options)
    {
        _documentComparer = documentComparer ?? throw new ArgumentNullException(nameof(documentComparer));
        _options = options;
    }

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

        JsonDocument? docX = JsonSerializer.Deserialize<JsonDocument>(x, _options);
        JsonDocument? docY = JsonSerializer.Deserialize<JsonDocument>(y, _options);
        return _documentComparer.Equals(docX, docY);
    }

    public int GetHashCode(string obj)
    {
        return obj.GetHashCode();
    }
}
