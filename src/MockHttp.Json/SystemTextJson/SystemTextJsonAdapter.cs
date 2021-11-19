using System.Text.Json;

namespace MockHttp.Json.SystemTextJson;

/// <summary>
/// A JSON adapter for <see cref="JsonSerializer" />.
/// </summary>
public sealed class SystemTextJsonAdapter : IJsonAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonAdapter" /> class using specified optional <paramref name="options" />.
    /// </summary>
    /// <param name="options">The JSON serializer options.</param>
    public SystemTextJsonAdapter(JsonSerializerOptions? options = null)
    {
        Options = options;
    }

    /// <summary>
    /// The serializer options.
    /// </summary>
    internal JsonSerializerOptions? Options { get; }

    /// <inheritdoc />
    public string Serialize(object? value)
    {
        return JsonSerializer.Serialize(value, Options);
    }
}
