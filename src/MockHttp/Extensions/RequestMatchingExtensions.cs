using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using MockHttp.Extensions;
using MockHttp.Http;
using MockHttp.Language;
using MockHttp.Matchers;
using MockHttp.Patterns;
using MockHttp.Request;
using static MockHttp.Http.UriExtensions;

namespace MockHttp;

/// <summary>
/// Extensions for <see cref="IRequestMatching" />.
/// </summary>
public static class RequestMatchingExtensions
{
    private static bool ContainsWildcard(this string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

#if NETSTANDARD2_0
        return value.Contains("*");
#else
        return value.Contains('*', StringComparison.InvariantCultureIgnoreCase);
#endif
    }

    /// <summary>
    /// Matches a request by specified <paramref name="requestUri" />.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="requestUri">The request URI or a URI wildcard.</param>
    /// <returns>The request matching builder instance.</returns>
#pragma warning disable CA1054
    public static IRequestMatching RequestUri(
        this IRequestMatching builder,
#if NET8_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.Uri)]
#endif
        string requestUri
    )
#pragma warning restore CA1054
    {
        return builder.RequestUri(requestUri, true);
    }

    /// <summary>
    /// Matches a request by specified <paramref name="requestUri" />.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="requestUri">The request URI or a URI wildcard.</param>
    /// <param name="allowWildcards"><see langword="true" /> to allow wildcards, or <see langword="false" /> if exact matching.</param>
    /// <returns>The request matching builder instance.</returns>
#pragma warning disable CA1054
    // For now, keep this internal. For coverage, and most likely, the API will change so then we'd have more to deprecate (using patterns).
    internal static IRequestMatching RequestUri(this IRequestMatching builder, string requestUri, bool allowWildcards)
#pragma warning restore CA1054
    {
        if (requestUri is null)
        {
            throw new ArgumentNullException(nameof(requestUri));
        }

        return allowWildcards && requestUri.ContainsWildcard()
            ? builder.RequestUri(Pattern.Wildcard(requestUri))
            : builder.RequestUri(new Uri(requestUri, DotNetRelativeOrAbsolute));
    }

    /// <summary>
    /// Matches a request by specified <paramref name="requestUri" />.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="requestUri">The relative or absolute request URI.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching RequestUri(this IRequestMatching builder, Uri requestUri)
    {
        if (requestUri is null)
        {
            throw new ArgumentNullException(nameof(requestUri));
        }

        return builder.RequestUri(GetAbsoluteOrRelativePattern(requestUri));

        static Pattern GetAbsoluteOrRelativePattern(Uri pattern)
        {
            Uri patternRooted = pattern.EnsureIsRooted();
            return new Pattern
            {
                Value = pattern.ToString(),
                IsMatch = input =>
                {
                    var uri = new Uri(input, DotNetRelativeOrAbsolute);
                    return IsAbsoluteUriMatch(patternRooted, uri) || IsRelativeUriMatch(patternRooted, uri);
                }
            };

            static bool IsAbsoluteUriMatch(Uri pattern, Uri uri)
            {
                return pattern.IsAbsoluteUri && uri.Equals(pattern);
            }

            static bool IsRelativeUriMatch(Uri pattern, Uri uri)
            {
                return !pattern.IsAbsoluteUri
                    && uri.IsBaseOf(pattern)
                    && uri.ToString().EndsWith(pattern.ToString(), StringComparison.Ordinal);
            }
        }
    }

    /// <summary>
    /// Matches a request by specified <paramref name="pattern" />.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="pattern">The pattern that must match the request URI.</param>
    /// <returns>The request matching builder instance.</returns>
    private static IRequestMatching RequestUri(this IRequestMatching builder, Pattern pattern)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new UriMatcher(pattern));
    }

    /// <summary>
    /// Matches a request by query string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="key">The query string parameter key.</param>
    /// <param name="value">The query string value.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching QueryString(this IRequestMatching builder, string key, string? value)
    {
        return builder.QueryString(
            key,
            value is null
                ? []
                : [value]
        );
    }

    /// <summary>
    /// Matches a request by query string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="key">The query string parameter key.</param>
    /// <param name="values">The query string values.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching QueryString(this IRequestMatching builder, string key, IEnumerable<string> values)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return builder.QueryString(new Dictionary<string, IEnumerable<string>> { { key, values } });
    }

    /// <summary>
    /// Matches a request by query string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="key">The query string parameter key.</param>
    /// <param name="values">The query string value.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching QueryString(this IRequestMatching builder, string key, params string[] values)
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        return builder.QueryString(key, values?.AsEnumerable() ?? []);
    }

    /// <summary>
    /// Matches a request by query string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="parameters">The query string parameters.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching QueryString(
        this IRequestMatching builder,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> parameters
    )
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new QueryStringMatcher(parameters));
    }

    /// <summary>
    /// Matches a request by query string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="parameters">The query string parameters.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching QueryString(this IRequestMatching builder, NameValueCollection parameters)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (parameters is null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        return builder.Add(new QueryStringMatcher(parameters.AsEnumerable()!));
    }

    /// <summary>
    /// Matches a request by query string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="queryString">The query string.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching QueryString(this IRequestMatching builder, string queryString)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrEmpty(queryString))
        {
            throw new ArgumentException("Specify a query string, or use 'WithoutQueryString'.", nameof(queryString));
        }

        return builder.Add(new QueryStringMatcher(queryString));
    }

    /// <summary>
    /// Matches a request explicitly that has no request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching WithoutQueryString(this IRequestMatching builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new QueryStringMatcher(""));
    }

    /// <summary>
    /// Matches a request by HTTP method.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Method(this IRequestMatching builder, string httpMethod)
    {
        if (httpMethod is null)
        {
            throw new ArgumentNullException(nameof(httpMethod));
        }

        return builder.Method(new HttpMethod(httpMethod));
    }

    /// <summary>
    /// Matches a request by HTTP method.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="method">The HTTP method.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Method(this IRequestMatching builder, HttpMethod method)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new HttpMethodMatcher(method));
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <param name="allowWildcards"><see langword="true" /> to allow wildcards, or <see langword="false" /> if exact matching.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Header(this IRequestMatching builder, string name, string value, bool allowWildcards)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new HttpHeadersMatcher(name, value, allowWildcards));
    }

    /// <summary>
    /// Matches a request by HTTP header. A type converter is used to convert the <paramref name="value" /> to string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Header<T>(this IRequestMatching builder, string name, T value)
        where T : struct
    {
        TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
        return builder.Header(name, converter.ConvertToString(value)!);
    }

    /// <summary>
    /// Matches a request by HTTP header on a datetime value (per RFC-2616).
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <param name="date">The header value.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Header(this IRequestMatching builder, string name, DateTime date)
    {
        return builder.Header(name, (DateTimeOffset)date.ToUniversalTime());
    }

    /// <summary>
    /// Matches a request by HTTP header on a datetime value (per RFC-2616).
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <param name="date">The header value.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Header(this IRequestMatching builder, string name, DateTimeOffset date)
    {
        // https://tools.ietf.org/html/rfc2616#section-3.3.1
        CultureInfo ci = CultureInfo.InvariantCulture;
        return builder.Header(name, date.ToString("R", ci));
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <param name="values">The header values.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Header(this IRequestMatching builder, string name, IEnumerable<string> values)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new HttpHeadersMatcher(name, values));
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <param name="values">The header values.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Header(this IRequestMatching builder, string name, params string[] values)
    {
        return builder.Header(name, values.AsEnumerable());
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Header(this IRequestMatching builder, string name)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new HttpHeadersMatcher(name));
    }

    /// <summary>
    /// Matches a request by HTTP headers.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="headers">The headers.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Headers(this IRequestMatching builder, string headers)
    {
        return builder.Headers(HttpHeadersCollection.Parse(headers));
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="headers">The headers.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Headers(this IRequestMatching builder, IEnumerable<KeyValuePair<string, string>> headers)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new HttpHeadersMatcher(headers));
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="headers">The headers.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Headers(
        this IRequestMatching builder,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers
    )
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new HttpHeadersMatcher(headers));
    }

    /// <summary>
    /// Matches a request by content type.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="mediaType">The content type.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching ContentType(this IRequestMatching builder, string mediaType)
    {
        if (mediaType is null)
        {
            throw new ArgumentNullException(nameof(mediaType));
        }

        return builder.ContentType(MediaTypeHeaderValue.Parse(mediaType));
    }

    /// <summary>
    /// Matches a request by content type.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="encoding">The content encoding.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching ContentType(this IRequestMatching builder, string contentType, Encoding encoding)
    {
        if (contentType is null)
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        if (encoding is null)
        {
            throw new ArgumentNullException(nameof(encoding));
        }

        var mediaType = new MediaTypeHeaderValue(contentType) { CharSet = encoding.WebName };
        return builder.ContentType(mediaType);
    }

    /// <summary>
    /// Matches a request by media type.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="mediaType">The media type.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching ContentType(this IRequestMatching builder, MediaTypeHeaderValue mediaType)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (mediaType is null)
        {
            throw new ArgumentNullException(nameof(mediaType));
        }

        return builder.Add(new MediaTypeHeaderMatcher(mediaType));
    }

    /// <summary>
    /// Matches a request by form data.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="key">The form data parameter key.</param>
    /// <param name="value">The form data value.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching FormData(this IRequestMatching builder, string key, string value)
    {
        return builder.FormData([new KeyValuePair<string, string>(key, value)]);
    }

    /// <summary>
    /// Matches a request by form data.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="formData">The form data parameters.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching FormData(this IRequestMatching builder, IEnumerable<KeyValuePair<string, string>> formData)
    {
        if (formData is null)
        {
            throw new ArgumentNullException(nameof(formData));
        }

        return builder.FormData(formData.Select(
            d => new KeyValuePair<string, IEnumerable<string>>(
                d.Key,
                d.Value == null! ? Array.Empty<string>() : new[] { d.Value })
        ));
    }

    /// <summary>
    /// Matches a request by form data.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="formData">The form data parameters.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching FormData(this IRequestMatching builder, NameValueCollection formData)
    {
        if (formData is null)
        {
            throw new ArgumentNullException(nameof(formData));
        }

        return builder.FormData(formData.AsEnumerable()!);
    }

    /// <summary>
    /// Matches a request by form data.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="formData">The form data parameters.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching FormData(
        this IRequestMatching builder,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> formData
    )
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new FormDataMatcher(formData));
    }

    /// <summary>
    /// Matches a request by form data.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="urlEncodedFormData">The form data.</param>
    /// <returns>The request matching builder instance.</returns>
#pragma warning disable CA1054 // Justification: the intent is that callee has full control over the encoding.
    public static IRequestMatching FormData(this IRequestMatching builder, string urlEncodedFormData)
#pragma warning restore CA1054
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrEmpty(urlEncodedFormData))
        {
            throw new ArgumentException("Specify the url encoded form data.", nameof(urlEncodedFormData));
        }

        return builder.Add(new FormDataMatcher(urlEncodedFormData));
    }

    /// <summary>
    /// Matches a request by the request body.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="body">The expected request body.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Body(this IRequestMatching builder, string body)
    {
        return builder.Body(body, MockHttpHandler.DefaultEncoding);
    }

    /// <summary>
    /// Matches a request by request body.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="body">The expected request body.</param>
    /// <param name="encoding">The request content encoding.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Body(this IRequestMatching builder, string body, Encoding encoding)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (body is null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        return builder.Add(new ContentMatcher(body, encoding));
    }

    /// <summary>
    /// Matches a request by request body.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="body">The expected request body.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Body(this IRequestMatching builder, byte[] body)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (body is null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        return builder.Add(new ContentMatcher(body));
    }

    /// <summary>
    /// Matches a request by request body.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="body">The expected request body.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Body(this IRequestMatching builder, Stream body)
    {
        if (body is null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        using var ms = new MemoryStream();
        body.CopyTo(ms);
        return builder.Body(ms.ToArray());
    }

    /// <summary>
    /// Matches a request explicitly that has no request body.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching WithoutBody(this IRequestMatching builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new ContentMatcher());
    }

    /// <summary>
    /// Matches a request by partially matching the request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="partialBody">The partial request body.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching PartialBody(this IRequestMatching builder, string partialBody)
    {
        return builder.PartialBody(partialBody, MockHttpHandler.DefaultEncoding);
    }

    /// <summary>
    /// Matches a request by partially matching the request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="partialBody">The partial request body.</param>
    /// <param name="encoding">The request content encoding.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching PartialBody(this IRequestMatching builder, string partialBody, Encoding encoding)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (partialBody is null)
        {
            throw new ArgumentNullException(nameof(partialBody));
        }

        return builder.Add(new PartialContentMatcher(partialBody, encoding));
    }

    /// <summary>
    /// Matches a request by partially matching the request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="partialBody">The partial request body.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching PartialBody(this IRequestMatching builder, byte[] partialBody)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (partialBody is null)
        {
            throw new ArgumentNullException(nameof(partialBody));
        }

        return builder.Add(new PartialContentMatcher(partialBody));
    }

    /// <summary>
    /// Matches a request by partially matching the request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="partialBody">The request content.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching PartialBody(this IRequestMatching builder, Stream partialBody)
    {
        if (partialBody is null)
        {
            throw new ArgumentNullException(nameof(partialBody));
        }

        using var ms = new MemoryStream();
        partialBody.CopyTo(ms);
        return builder.PartialBody(ms.ToArray());
    }

    /// <summary>
    /// Matches a request by verifying it against a list of constraints, for which at least one has to match the request.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="anyBuilder">An action to configure an inner request matching builder.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Any(this IRequestMatching builder, Action<IRequestMatching> anyBuilder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (anyBuilder is null)
        {
            throw new ArgumentNullException(nameof(anyBuilder));
        }

        var anyIRequestMatchBuilder = new AnyRequestMatching();
        anyBuilder(anyIRequestMatchBuilder);
        return builder.Add(new AnyMatcher(anyIRequestMatchBuilder.Build()));
    }

    /// <summary>
    /// Matches a request using a custom expression.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="expression">The expression.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Where(this IRequestMatching builder, Expression<Func<HttpRequestMessage, bool>> expression)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new ExpressionMatcher(expression));
    }

    /// <summary>
    /// Matches a request by matching the request message version.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="version">The message version.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Version(this IRequestMatching builder, string version)
    {
        if (version is null)
        {
            throw new ArgumentNullException(nameof(version));
        }

        return builder.Version(System.Version.Parse(version));
    }

    /// <summary>
    /// Matches a request by matching the request message version.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="version">The message version.</param>
    /// <returns>The request matching builder instance.</returns>
    public static IRequestMatching Version(this IRequestMatching builder, Version version)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Add(new VersionMatcher(version));
    }
}
