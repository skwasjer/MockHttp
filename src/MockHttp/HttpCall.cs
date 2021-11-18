using System.Collections.ObjectModel;
using System.Text;
using MockHttp.Matchers;
using MockHttp.Responses;

namespace MockHttp;

/// <summary>
/// Contains the setup that controls the behavior of a mocked HTTP request.
/// </summary>
internal class HttpCall
{
    private IResponseStrategy _responseStrategy;
    private string _verifiableBecause;
    private IReadOnlyCollection<IAsyncHttpRequestMatcher> _matchers;

    public IReadOnlyCollection<IAsyncHttpRequestMatcher> Matchers
    {
        get
        {
            if (_matchers is null)
            {
                SetMatchers(new List<IAsyncHttpRequestMatcher>());
            }

            return _matchers;
        }
    }

    public virtual bool IsVerifiable { get; private set; }

    public virtual bool IsVerified { get; protected set; }

    public virtual bool IsInvoked { get; protected set; }

    public Action<HttpRequestMessage> Callback { get; private set; }

    public virtual async Task<HttpResponseMessage> SendAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
    {
        IsInvoked = true;

        if (_responseStrategy is null)
        {
            // TODO: clarify which mock.
            throw new HttpMockException("No response configured for mock.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        Callback?.Invoke(requestContext.Request);
        HttpResponseMessage responseMessage = await _responseStrategy.ProduceResponseAsync(requestContext, cancellationToken).ConfigureAwait(false);
        responseMessage.RequestMessage = requestContext.Request;
        return responseMessage;
    }

    public virtual void SetResponse(IResponseStrategy responseStrategy)
    {
        _responseStrategy = responseStrategy;
    }

    public virtual void SetMatchers(IEnumerable<IAsyncHttpRequestMatcher> matchers)
    {
        if (matchers is null)
        {
            throw new ArgumentNullException(nameof(matchers));
        }

        _matchers = new ReadOnlyCollection<IAsyncHttpRequestMatcher>(matchers.ToList());
    }

    public virtual void SetCallback(Action<HttpRequestMessage> callback)
    {
        Callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public virtual void SetVerifiable(string because)
    {
        IsVerifiable = true;
        _verifiableBecause = because;
    }

    public override string ToString()
    {
        if (_matchers is null || _matchers.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (IAsyncHttpRequestMatcher m in _matchers)
        {
            sb.Append(m);
            sb.Append(", ");
        }

        sb.Remove(sb.Length - 2, 2);

        return sb.ToString();
    }

    public virtual bool VerifyIfInvoked()
    {
        if (IsInvoked)
        {
            IsVerified = true;
        }

        return IsVerified;
    }

    public virtual void Reset()
    {
        Uninvoke();
        IsVerified = false;
        IsVerifiable = false;
        _verifiableBecause = null;
        _responseStrategy = null;
        Callback = null;
        _matchers = null;
    }

    public virtual void Uninvoke()
    {
        IsInvoked = false;
    }
}
