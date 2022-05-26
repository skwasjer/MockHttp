namespace MockHttp.Json;

/// <summary>
/// Configuration extensions.
/// </summary>
public static class MockConfigurationExtensions
{
    /// <summary>
    /// Registers the specified <paramref name="jsonAdapter" />.
    /// </summary>
    /// <param name="mockConfig">The mock configuration object.</param>
    /// <param name="jsonAdapter">The JSON adapter.</param>
    /// <returns>The instance to continue chaining configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mockConfig" /> or <paramref name="jsonAdapter" /> is <see langword="null" />.</exception>
    public static IMockConfiguration UseJsonAdapter(this IMockConfiguration mockConfig, IJsonAdapter jsonAdapter)
    {
        if (mockConfig is null)
        {
            throw new ArgumentNullException(nameof(mockConfig));
        }

        if (jsonAdapter is null)
        {
            throw new ArgumentNullException(nameof(jsonAdapter));
        }

        return mockConfig.Use(jsonAdapter);
    }
}
