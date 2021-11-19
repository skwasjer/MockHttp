using System.Text.Json;

namespace MockHttp.Json.SystemTextJson;

/// <summary>
/// Configuration extensions.
/// </summary>
public static class MockConfigurationExtensions
{
    /// <summary>
    /// Registers the <see cref="SystemTextJsonAdapter"/> as the default adapter.
    /// </summary>
    /// <param name="mockConfig">The mock configuration object.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>The instance to continue chaining configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mockConfig" /> is <see langword="null" />.</exception>
    public static IMockConfiguration UseSystemTextJson(this IMockConfiguration mockConfig, JsonSerializerOptions? options = null)
    {
        if (mockConfig is null)
        {
            throw new ArgumentNullException(nameof(mockConfig));
        }

        return mockConfig.UseJsonAdapter(new SystemTextJsonAdapter(options));
    }
}
