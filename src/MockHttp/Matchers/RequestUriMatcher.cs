﻿using System.Diagnostics;
using MockHttp.Http;
using MockHttp.Patterns;
using MockHttp.Responses;
using static MockHttp.Http.UriExtensions;

namespace MockHttp.Matchers;

/// <summary>
/// Matches a request by the request URI.
/// </summary>
[Obsolete($"Replaced with {nameof(UriMatcher)}. Will be removed in next major release.")]
public class RequestUriMatcher : HttpRequestMatcher
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Uri _requestUri = default!;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string _formattedUri;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly WildcardPattern? _uriPatternMatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestUriMatcher" /> class using specified <paramref name="uri" />.
    /// </summary>
    /// <param name="uri">The request URI.</param>
    public RequestUriMatcher(Uri uri)
    {
        _requestUri = uri.EnsureIsRooted();
        _formattedUri = _requestUri.ToString();
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
            _uriPatternMatcher = WildcardPattern.Create(uriString);
        }
        else
        {
            // If no wildcards, then must be actual uri.
            _requestUri = new Uri(uriString, DotNetRelativeOrAbsolute).EnsureIsRooted();
            _formattedUri = _requestUri.ToString();
        }
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

        return _uriPatternMatcher.Value.IsMatch(requestUri.ToString());
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
