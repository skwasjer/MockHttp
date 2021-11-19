using Newtonsoft.Json;

namespace MockHttp.Json.Newtonsoft;

/// <summary>
/// A JSON adapter for <see cref="JsonSerializer" />.
/// </summary>
public class NewtonsoftAdapter : IJsonAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NewtonsoftAdapter" /> class using specified optional <paramref name="settings" />.
    /// </summary>
    /// <param name="settings">The JSON serializer settings.</param>
    public NewtonsoftAdapter(JsonSerializerSettings? settings = null)
    {
        Settings = settings;
    }

    /// <summary>
    /// Gets the serializer settings.
    /// </summary>
    internal JsonSerializerSettings? Settings { get; }

    /// <inheritdoc />
    public string Serialize(object? value)
    {
        return JsonConvert.SerializeObject(value, Settings);
    }
}
