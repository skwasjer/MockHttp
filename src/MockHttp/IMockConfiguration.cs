namespace MockHttp;

/// <summary>
/// Allows to register custom services for use by matchers and/or response strategies.
/// </summary>
public interface IMockConfiguration
{
    /// <summary>
    /// Registers a service for use by matchers and/or response strategies.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="service">The service.</param>
    /// <returns>The configuration instance.</returns>
    IMockConfiguration Use<TService>(TService service);

    /// <summary>
    /// Gets the registered items.
    /// </summary>
    IReadOnlyDictionary<Type, object> Items { get; }
}
