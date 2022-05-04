using MockHttp.Responses;

// ReSharper disable once CheckNamespace : BREAKING - change namespace with next release. (remove Extensions)
namespace MockHttp.Json.Extensions;

internal static class MockHttpRequestContextExtensions
{
    /// <summary>
    /// Gets the adapter from the request context as configured on the mock handler via <see cref="MockConfigurationExtensions.UseJsonAdapter" />, or if none is registered returns the default adapter (Newtonsoft).
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <returns>The adapter.</returns>
    internal static IJsonAdapter GetAdapter(this MockHttpRequestContext context)
    {
        context.TryGetService<IJsonAdapter>(out IJsonAdapter? adapter);
        return adapter ?? Defaults.Adapter;
    }
}
