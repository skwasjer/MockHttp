using System.Diagnostics;
using MockHttp.Responses;

namespace MockHttp.Matchers;

/// <summary>
/// Matches a request by the request URI.
/// </summary>
public class RequestUriMatcher : HttpRequestMatcher
{
    private static readonly UriKind DotNetRelativeOrAbsolute = Type.GetType("Mono.Runtime") == null ? UriKind.RelativeOrAbsolute : (UriKind)300;

    private const char UriSegmentDelimiter = '/';

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Uri _requestUri = default!;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _formattedUri = default!;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly PatternMatcher? _uriPatternMatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestUriMatcher" /> class using specified <paramref name="uri" />.
    /// </summary>
    /// <param name="uri">The request URI.</param>
    public RequestUriMatcher(Uri uri)
    {
        SetRequestUri(uri);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestUriMatcher" /> class using specified <paramref name="uriString" />.
    /// </summary>
    /// <param name="uriString">The request URI or a URI wildcard.</param>
    /// <param name="allowWildcards"><see langword="true" /> to allow wildcards, or <see langword="false" /> if exact matching.</param>
    public RequestUriMatcher(string uriString, bool allowWildcards = true)
    {
        _formattedUri = uriString ?? throw new ArgumentNullException(nameof(uriString));

        if (allowWildcards
#if NETSTANDARD2_0 || NETFRAMEWORK
         && uriString.Contains("*")
#else
         && uriString.Contains('*', StringComparison.InvariantCultureIgnoreCase)
#endif
           )
        {
            _uriPatternMatcher = new RegexPatternMatcher(uriString);
        }
        else
        {
            // If no wildcards, then must be actual uri.
            SetRequestUri(new Uri(uriString, DotNetRelativeOrAbsolute));
        }
    }

    private void SetRequestUri(Uri uri)
    {
        _requestUri = uri ?? throw new ArgumentNullException(nameof(uri));

        if (!_requestUri.IsAbsoluteUri)
        {
            string relUri = _requestUri.ToString();
            if (relUri.Length > 0 && _requestUri.ToString()[0] != UriSegmentDelimiter)
            {
                _requestUri = new Uri($"{UriSegmentDelimiter}{_requestUri}", UriKind.Relative);
            }
        }

        _formattedUri = _requestUri.ToString();
    }

    /// <inheritdoc />
    public override bool IsMatch(MockHttpRequestContext requestContext)
    {
        if (requestContext is null)
        {
            throw new ArgumentNullException(nameof(requestContext));
        }

        Uri? requestUri = requestContext.Request.RequestUri;
        if (requestUri is null)
        {
            return false;
        }

        if (_uriPatternMatcher is null)
        {
            return IsAbsoluteUriMatch(requestUri) || IsRelativeUriMatch(requestUri);
        }

        return _uriPatternMatcher.IsMatch(requestUri.ToString());
    }

    private bool IsAbsoluteUriMatch(Uri uri)
    {
        return _requestUri.IsAbsoluteUri && uri.Equals(_requestUri);
    }

    private bool IsRelativeUriMatch(Uri uri)
    {
        return !_requestUri.IsAbsoluteUri
         && uri.IsBaseOf(_requestUri)
         && uri.ToString().EndsWith(_requestUri.ToString(), StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"RequestUri: '{_formattedUri}'";
    }
}
