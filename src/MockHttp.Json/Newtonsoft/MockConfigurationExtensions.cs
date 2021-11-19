using Newtonsoft.Json;

namespace MockHttp.Json.Newtonsoft;

/// <summary>
/// Configuration extensions.
/// </summary>
public static class MockConfigurationExtensions
{
    /// <summary>
    /// Registers the specified <paramref name="serializerSettings" /> with the <see cref="NewtonsoftAdapter"/> as the default adapter.
    /// </summary>
    /// <param name="mockConfig">The mock configuration object.</param>
    /// <param name="serializerSettings">The serializer settings.</param>
    /// <returns>The instance to continue chaining configuration.</returns>
    public static IMockConfiguration UseNewtonsoftJson(this IMockConfiguration mockConfig, JsonSerializerSettings? serializerSettings = null)
    {
        if (mockConfig is null)
        {
            throw new ArgumentNullException(nameof(mockConfig));
        }

        return mockConfig.UseJsonAdapter(new NewtonsoftAdapter(serializerSettings));
    }
}
