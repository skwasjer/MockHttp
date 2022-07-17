namespace MockHttp.Responses;

/// <summary>
/// A delegate which when executed returns a configured HTTP response.
/// </summary>
/// <returns></returns>
public delegate Task ResponseHandlerDelegate(MockHttpRequestContext requestContext, HttpResponseMessage responseMessage, CancellationToken cancellationToken);

/// <summary>
/// Describes a way to apply a response behavior in a response builder pipeline.
/// </summary>
public interface IResponseBehavior
{
    /// <summary>
    /// Executes the behavior. Call <paramref name="next" /> to execute the next behavior in the response pipeline and use its returned response message.
    /// </summary>
    /// <param name="requestContext">The current request context.</param>
    /// <param name="responseMessage">The response message.</param>
    /// <param name="next">The next behavior.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An awaitable that upon completion returns the HTTP response message.</returns>
    Task HandleAsync(MockHttpRequestContext requestContext, HttpResponseMessage responseMessage, ResponseHandlerDelegate next, CancellationToken cancellationToken);
}
