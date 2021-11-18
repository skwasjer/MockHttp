namespace MockHttp.Responses;

/// <summary>
/// Represents a strategy that produces a mocked response.
/// </summary>
public interface IResponseStrategy
{
    /// <summary>
    /// Produces a response message to return for the <paramref name="requestContext" />.
    /// </summary>
    /// <param name="requestContext">The request message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An awaitable that when completed provides the response message.</returns>
    Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken);
}
