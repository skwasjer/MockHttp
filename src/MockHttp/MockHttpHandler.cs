using System.Collections.ObjectModel;
using System.Net;
using MockHttp.Language;
using MockHttp.Language.Flow;
using MockHttp.Matchers;
using MockHttp.Responses;
using MockHttp.Threading;

namespace MockHttp;

/// <summary>
/// Represents a message handler that can be used to mock HTTP responses and verify HTTP requests sent via <see cref="HttpClient" />.
/// </summary>
public sealed class MockHttpHandler : HttpMessageHandler, IMockConfiguration
{
    private readonly ConcurrentCollection<HttpCall> _setups;
    private readonly HttpCall _fallbackSetup;
    private readonly IDictionary<Type, object> _items;
    private readonly ReadOnlyDictionary<Type, object> _readOnlyItems;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpHandler" /> class.
    /// </summary>
    public MockHttpHandler()
    {
        _setups = new ConcurrentCollection<HttpCall>();
        InvokedRequests = new InvokedHttpRequestCollection(this);
        _items = new Dictionary<Type, object>();
        _readOnlyItems = new ReadOnlyDictionary<Type, object>(_items);

        _fallbackSetup = new HttpCall();
        Fallback = new FallbackRequestSetupPhrase(_fallbackSetup);
        Reset();
    }

    /// <summary>
    /// Gets a collection of invoked requests that were handled.
    /// </summary>
    public IInvokedHttpRequestCollection InvokedRequests { get; }

    /// <summary>
    /// Gets a fallback configurer that can be used to configure the default response if no expectation was matched.
    /// </summary>
    public IRespondsThrows Fallback { get; }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        await LoadIntoBufferAsync(request.Content).ConfigureAwait(false);

        var requestContext = new MockHttpRequestContext(request, _readOnlyItems);
        foreach (HttpCall setup in _setups.Reverse())
        {
            if (await setup.Matchers.AllAsync(requestContext).ConfigureAwait(false))
            {
                return await SendAsync(setup, requestContext, cancellationToken).ConfigureAwait(false);
            }
        }

        return await SendAsync(_fallbackSetup, requestContext, cancellationToken).ConfigureAwait(false);
    }

    private Task<HttpResponseMessage> SendAsync(HttpCall setup, MockHttpRequestContext requestContext, CancellationToken cancellationToken)
    {
        ((InvokedHttpRequestCollection)InvokedRequests).Add(new InvokedHttpRequest(setup, requestContext.Request));
        return setup.SendAsync(requestContext, cancellationToken);
    }

    /// <summary>
    /// Configures a condition for an expectation. If the condition evaluates to <see langword="false" />, the expectation is not matched.
    /// </summary>
    /// <param name="matching">The request match builder.</param>
    /// <returns>The configured request.</returns>
    public IConfiguredRequest When(Action<RequestMatching> matching)
    {
        if (matching is null)
        {
            throw new ArgumentNullException(nameof(matching));
        }

        var b = new RequestMatching();
        matching(b);

        var newSetup = new HttpCallSequence();
        newSetup.SetMatchers(b.Build());
        _setups.Add(newSetup);
        return new HttpRequestSetupPhrase(newSetup);
    }

    /// <summary>
    /// Resets this mock's state. This includes its setups, configured default return values, and all recorded invocations.
    /// </summary>
    public void Reset()
    {
        InvokedRequests.Clear();
        _fallbackSetup.Reset();
        _setups.Clear();

        Fallback.Respond(_ => CreateDefaultResponse());
    }

    /// <summary>
    /// Verifies that a request matching the specified match conditions has been sent.
    /// </summary>
    /// <param name="matching">The conditions to match.</param>
    /// <param name="times">The number of times a request is allowed to be sent.</param>
    /// <param name="because">The reasoning for this expectation.</param>
    public void Verify(Action<RequestMatching> matching, Func<IsSent> times, string because = null)
    {
        Verify(matching, times?.Invoke(), because);
    }

    /// <summary>
    /// Verifies that a request matching the specified match conditions has been sent.
    /// </summary>
    /// <param name="matching">The conditions to match.</param>
    /// <param name="times">The number of times a request is allowed to be sent.</param>
    /// <param name="because">The reasoning for this expectation.</param>
    /// <remarks>
    /// When verifying <see cref="HttpContent" /> using a <see cref="ContentMatcher" /> use the <see cref="VerifyAsync(System.Action{MockHttp.RequestMatching},System.Func{MockHttp.IsSent},string)" /> overload to prevent potential deadlocks.
    /// </remarks>
    public void Verify(Action<RequestMatching> matching, IsSent times, string because = null)
    {
        TaskHelpers.RunSync(() => VerifyAsync(matching, times, because), TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Verifies that a request matching the specified match conditions has been sent.
    /// </summary>
    /// <param name="matching">The conditions to match.</param>
    /// <param name="times">The number of times a request is allowed to be sent.</param>
    /// <param name="because">The reasoning for this expectation.</param>
    public Task VerifyAsync(Action<RequestMatching> matching, Func<IsSent> times, string because = null)
    {
        return VerifyAsync(matching, times?.Invoke(), because);
    }

    /// <summary>
    /// Verifies that a request matching the specified match conditions has been sent.
    /// </summary>
    /// <param name="matching">The conditions to match.</param>
    /// <param name="times">The number of times a request is allowed to be sent.</param>
    /// <param name="because">The reasoning for this expectation.</param>
    public async Task VerifyAsync(Action<RequestMatching> matching, IsSent times, string because = null)
    {
        if (matching is null)
        {
            throw new ArgumentNullException(nameof(matching));
        }

        times ??= IsSent.AtLeastOnce();

        var rm = new RequestMatching();
        matching(rm);

        IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = rm.Build();
        IReadOnlyList<IInvokedHttpRequest> matchedRequests = InvokedRequests;
        if (matchers.Count > 0)
        {
            var list = new List<IInvokedHttpRequest>();
            foreach (IInvokedHttpRequest invokedHttpRequest in InvokedRequests)
            {
                var requestContext = new MockHttpRequestContext(invokedHttpRequest.Request);
                if (await matchers.AllAsync(requestContext).ConfigureAwait(false))
                {
                    list.Add(invokedHttpRequest);
                }
            }

            matchedRequests = list;
        }

        if (!times.Verify(matchedRequests.Count))
        {
            throw new HttpMockException(times.GetErrorMessage(matchedRequests.Count, BecauseMessage(because)));
        }

        foreach (InvokedHttpRequest r in matchedRequests.Cast<InvokedHttpRequest>())
        {
            r.MarkAsVerified();
        }
    }

    /// <summary>
    /// Verifies that all verifiable expected requests have been sent.
    /// </summary>
    public void Verify()
    {
        IEnumerable<HttpCall> verifiableSetups = _setups.Where(r => r.IsVerifiable);

        Verify(verifiableSetups);
    }

    /// <summary>
    /// Verifies all expected requests regardless of whether they have been flagged as verifiable.
    /// </summary>
    public void VerifyAll()
    {
        Verify(_setups);
    }

    /// <summary>
    /// Verifies that there were no requests sent other than those already verified.
    /// </summary>
    [Obsolete("Renamed to " + nameof(VerifyNoOtherRequests) + ". This method will be removed in future.")]
    public void VerifyNoOtherCalls()
    {
        VerifyNoOtherRequests();
    }

    /// <summary>
    /// Verifies that there were no requests sent other than those already verified.
    /// </summary>
    public void VerifyNoOtherRequests()
    {
        var unverifiedRequests = InvokedRequests
            .Cast<InvokedHttpRequest>()
            .Where(r => !r.IsVerified)
            .ToList();
        if (!unverifiedRequests.Any())
        {
            return;
        }

        string unverifiedRequestsStr = string.Join(Environment.NewLine, unverifiedRequests.Select(ir => ir.Request.ToString()));

        throw new HttpMockException($"There are {unverifiedRequests.Count} unverified requests:{Environment.NewLine}{unverifiedRequestsStr}");
    }

    IMockConfiguration IMockConfiguration.Use<TService>(TService service)
    {
        _items[typeof(TService)] = service;
        return this;
    }

    IReadOnlyDictionary<Type, object> IMockConfiguration.Items => _readOnlyItems;

    private void Verify(IEnumerable<HttpCall> verifiableSetups)
    {
        var expectedInvocations = verifiableSetups
            .Where(setup => !setup.VerifyIfInvoked())
            .ToList();
        if (!expectedInvocations.Any())
        {
            return;
        }

        string invokedRequestsStr = string.Join(Environment.NewLine, InvokedRequests.Select(ir => ir.Request.ToString()));
        if (invokedRequestsStr.Length > 0)
        {
            invokedRequestsStr = Environment.NewLine + "Seen requests: " + invokedRequestsStr;
        }

        throw new HttpMockException($"There are {expectedInvocations.Count} unfulfilled expectations:{Environment.NewLine}{string.Join(Environment.NewLine, expectedInvocations.Select(r => '\t' + r.ToString()))}{invokedRequestsStr}");
    }

    private static HttpResponseMessage CreateDefaultResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = "No request is configured, returning default response." };
    }

    private static async Task LoadIntoBufferAsync(HttpContent httpContent)
    {
        if (httpContent is not null)
        {
            // Force read content, so content can be checked more than once.
            await httpContent.LoadIntoBufferAsync().ConfigureAwait(false);
            // Force read content length, in case it will be checked via header matcher.
            // ReSharper disable once UnusedVariable
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            long? cl = httpContent.Headers.ContentLength;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        }
    }

    private static string BecauseMessage(string because)
    {
        if (string.IsNullOrWhiteSpace(because))
        {
            return string.Empty;
        }

        because = because.TrimStart(' ');
        return because.StartsWith("because", StringComparison.OrdinalIgnoreCase)
            ? " " + because
            : " because " + because;
    }

    internal void UninvokeAll()
    {
        foreach (HttpCall setup in _setups)
        {
            setup.Uninvoke();
        }
    }
}
