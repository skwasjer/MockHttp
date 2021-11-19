namespace MockHttp.Json;

/// <summary>
/// An adapter for JSON (de)serialization and comparison.
/// </summary>
public interface IJsonAdapter
{
    /// <summary>
    /// Serializes the specified <paramref name="value" /> to JSON.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The JSON representation of <paramref name="value" />.</returns>
    string Serialize(object? value);
}
