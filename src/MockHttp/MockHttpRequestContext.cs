using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace MockHttp;

/// <summary>
/// Represents the mocked request context.
/// </summary>
public class MockHttpRequestContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpRequestContext" /> class.
    /// </summary>
    /// <param name="request">The request message.</param>
    /// <param name="services">Optional registered services for use by matchers and/or response strategies.</param>
    public MockHttpRequestContext(HttpRequestMessage request, IReadOnlyDictionary<Type, object>? services = null)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Services = services ?? new ReadOnlyDictionary<Type, object>(new Dictionary<Type, object>());
    }

    /// <summary>
    /// Gets the request.
    /// </summary>
    public HttpRequestMessage Request { get; }

    /// <summary>
    /// Gets a dictionary of services added during request setup.
    /// </summary>
    public IReadOnlyDictionary<Type, object> Services { get; }

    /// <summary>
    /// Tries to resolve a service registered with <see cref="IMockConfiguration" />.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="service">Returns the service.</param>
    /// <returns>true if the service is resolved, false otherwise.</returns>
    public bool TryGetService<TService>([NotNullWhen(true)] out TService? service)
    {
        if (Services.TryGetValue(typeof(TService), out object? s) && s != null!)
        {
            service = (TService)s;
            return true;
        }

        service = default;
        return false;
    }
}
