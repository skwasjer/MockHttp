﻿using System.Net.Http.Headers;
using MockHttp.Http;
using MockHttp.Patterns;
using MockHttp.Responses;

namespace MockHttp.Matchers;

/// <summary>
/// Matches a request by the request headers.
/// </summary>
public class HttpHeadersMatcher : ValueMatcher<HttpHeaders>
{
    private readonly HttpHeaderEqualityComparer _equalityComparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeadersMatcher" /> class using specified header <paramref name="name" />.
    /// </summary>
    /// <param name="name">The header name to match.</param>
    public HttpHeadersMatcher(string name) : base(new HttpHeadersCollection())
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        _equalityComparer = new HttpHeaderEqualityComparer(HttpHeaderMatchType.HeaderNameOnly);
        Value.TryAddWithoutValidation(name, (string?)null);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeadersMatcher" /> class using specified header <paramref name="name" /> and <paramref name="value" />.
    /// </summary>
    /// <param name="name">The header name to match.</param>
    /// <param name="value">The header value to match.</param>
    /// <param name="allowWildcards"><see langword="true" /> to allow wildcards, or <see langword="false" /> if exact matching.</param>
    public HttpHeadersMatcher(string name, string value, bool allowWildcards = false)
        : base(new HttpHeadersCollection())
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        _equalityComparer = allowWildcards
            ? new HttpHeaderEqualityComparer(WildcardPattern.Create(value))
            : new HttpHeaderEqualityComparer(HttpHeaderMatchType.HeaderNameAndPartialValues);

        Value.Add(name, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeadersMatcher" /> class using specified header <paramref name="name" /> and <paramref name="values" />.
    /// </summary>
    /// <param name="name">The header name to match.</param>
    /// <param name="values">The header values to match.</param>
    public HttpHeadersMatcher(string name, IEnumerable<string> values)
        : base(new HttpHeadersCollection())
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        _equalityComparer = new HttpHeaderEqualityComparer(HttpHeaderMatchType.HeaderNameAndPartialValues);

        Value.Add(name, values);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeadersMatcher" /> class using specified <paramref name="headers" />.
    /// </summary>
    /// <param name="headers">The headers to match.</param>
    public HttpHeadersMatcher(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        : base(new HttpHeadersCollection())
    {
        if (headers is null)
        {
            throw new ArgumentNullException(nameof(headers));
        }

        _equalityComparer = new HttpHeaderEqualityComparer(HttpHeaderMatchType.HeaderNameAndPartialValues);

        foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
        {
            Value.Add(header.Key, header.Value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeadersMatcher" /> class using specified <paramref name="headers" />.
    /// </summary>
    /// <param name="headers">The headers to match.</param>
    public HttpHeadersMatcher(IEnumerable<KeyValuePair<string, string>> headers)
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        : this(headers?.ToDictionary(h => h.Key, h => Enumerable.Repeat(h.Value, 1))!)
    {
    }

    /// <inheritdoc />
    public override bool IsMatch(MockHttpRequestContext requestContext)
    {
        return Value.All(h => IsMatch(h, requestContext.Request.Headers) || IsMatch(h, requestContext.Request.Content?.Headers));
    }

    /// <inheritdoc />
    public override bool IsExclusive => false;

    /// <inheritdoc />
    public override string ToString()
    {
        string value = Value.ToString();
#if !NET6_0_OR_GREATER
        value = value.Replace(
            "\r\n",
            Environment.NewLine
#if NETSTANDARD2_1
            , StringComparison.OrdinalIgnoreCase
#endif
        );
#endif

        return $"Headers: {value.TrimEnd('\r', '\n')}";
    }

    private bool IsMatch(KeyValuePair<string, IEnumerable<string>> expectedHeader, HttpHeaders? headers)
    {
        return headers is not null
         && headers.TryGetValues(expectedHeader.Key, out IEnumerable<string>? vls)
         && _equalityComparer.Equals(
                expectedHeader,
                new KeyValuePair<string, IEnumerable<string>>(expectedHeader.Key, vls)
            );
    }
}
