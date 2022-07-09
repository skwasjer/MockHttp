using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using MockHttp.Http;
using MockHttp.Matchers;

namespace MockHttp;

/// <summary>
/// Extensions for <see cref="RequestMatching" />.
/// </summary>
public static class RequestMatchingExtensions
{
    /// <summary>
    /// Matches a request by specified <paramref name="requestUri" />.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="requestUri">The request URI or a URI wildcard.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete("Use `RequestUri` instead, will be removed.")]
    public static RequestMatching Url(this RequestMatching builder, string requestUri)
    {
        return builder.RequestUri(requestUri);
    }

    /// <summary>
    /// Matches a request by specified <paramref name="requestUri" />.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="requestUri">The request URI or a URI wildcard.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching RequestUri(this RequestMatching builder, string requestUri)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (requestUri is null)
        {
            throw new ArgumentNullException(nameof(requestUri));
        }

        return builder.With(new RequestUriMatcher(requestUri, true));
    }

    /// <summary>
    /// Matches a request by specified <paramref name="requestUri" />.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="requestUri">The request URI or a URI wildcard.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete("Use `RequestUri` instead, will be removed.")]
    public static RequestMatching Url(this RequestMatching builder, Uri requestUri)
    {
        return builder.RequestUri(requestUri);
    }

    /// <summary>
    /// Matches a request by specified <paramref name="requestUri" />.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="requestUri">The relative or absolute request URI.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching RequestUri(this RequestMatching builder, Uri requestUri)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (requestUri is null)
        {
            throw new ArgumentNullException(nameof(requestUri));
        }

        return builder.With(new RequestUriMatcher(requestUri));
    }

    /// <summary>
    /// Matches a request by query string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="key">The query string parameter key.</param>
    /// <param name="value">The query string value.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching QueryString(this RequestMatching builder, string key, string value)
    {
        return builder.QueryString(
            key,
            value is null
                ? Array.Empty<string>()
                : new[] { value });
    }

    /// <summary>
    /// Matches a request by query string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="key">The query string parameter key.</param>
    /// <param name="values">The query string values.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching QueryString(this RequestMatching builder, string key, IEnumerable<string> values)
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
    public static RequestMatching QueryString(this RequestMatching builder, string key, params string[] values)
    {
        return builder.QueryString(key, values?.AsEnumerable());
    }

    /// <summary>
    /// Matches a request by query string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="parameters">The query string parameters.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching QueryString(this RequestMatching builder, IEnumerable<KeyValuePair<string, IEnumerable<string>>> parameters)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new QueryStringMatcher(parameters));
    }

    /// <summary>
    /// Matches a request by query string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="parameters">The query string parameters.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching QueryString(this RequestMatching builder, NameValueCollection parameters)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new QueryStringMatcher(parameters?.AsEnumerable()));
    }

    /// <summary>
    /// Matches a request by query string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="queryString">The query string.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching QueryString(this RequestMatching builder, string queryString)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrEmpty(queryString))
        {
            throw new ArgumentException("Specify a query string, or use 'WithoutQueryString'.", nameof(queryString));
        }

        return builder.With(new QueryStringMatcher(queryString));
    }

    /// <summary>
    /// Matches a request explicitly that has no request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching WithoutQueryString(this RequestMatching builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new QueryStringMatcher(""));
    }

    /// <summary>
    /// Matches a request by HTTP method.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Method(this RequestMatching builder, string httpMethod)
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
    public static RequestMatching Method(this RequestMatching builder, HttpMethod method)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new HttpMethodMatcher(method));
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <param name="allowWildcards"><see langword="true" /> to allow wildcards, or <see langword="false" /> if exact matching.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Header(this RequestMatching builder, string name, string value, bool allowWildcards)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new HttpHeadersMatcher(name, value, allowWildcards));
    }

    /// <summary>
    /// Matches a request by HTTP header. A type converter is used to convert the <paramref name="value" /> to string.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Header<T>(this RequestMatching builder, string name, T value)
        where T : struct
    {
        TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
        return builder.Header(name, converter.ConvertToString(value));
    }

    /// <summary>
    /// Matches a request by HTTP header on a datetime value (per RFC-2616).
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <param name="date">The header value.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Header(this RequestMatching builder, string name, DateTime date)
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
    public static RequestMatching Header(this RequestMatching builder, string name, DateTimeOffset date)
    {
        // https://tools.ietf.org/html/rfc2616#section-3.3.1
        CultureInfo ci = CultureInfo.InvariantCulture;
#if NETFRAMEWORK && NET452
			// .NET Framework does not normalize other common date formats,
			// so we use multiple matches.
			return builder.Any(any => any
				.Header(name, date.ToString("R", ci))
				.Header(name, date.ToString("dddd, dd-MMM-yy HH:mm:ss 'GMT'", ci))	// RFC 1036
				.Header(name, date.ToString("ddd MMM  d  H:mm:ss yyyy", ci))		// ANSI C's asctime()
			);
#else
        return builder.Header(name, date.ToString("R", ci));
#endif
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <param name="values">The header values.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Header(this RequestMatching builder, string name, IEnumerable<string> values)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new HttpHeadersMatcher(name, values));
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <param name="values">The header values.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Header(this RequestMatching builder, string name, params string[] values)
    {
        return builder.Header(name, values.AsEnumerable());
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Header(this RequestMatching builder, string name)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new HttpHeadersMatcher(name));
    }

    /// <summary>
    /// Matches a request by HTTP headers.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="headers">The headers.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Headers(this RequestMatching builder, string headers)
    {
        return builder.Headers(HttpHeadersCollection.Parse(headers));
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="name">The header name.</param>
    /// <param name="values">The header values.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete("Renamed to Header(). Will be removed in future version.")]
    public static RequestMatching Headers(this RequestMatching builder, string name, params string[] values)
    {
        return builder.Header(name, values);
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="headers">The headers.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Headers(this RequestMatching builder, IEnumerable<KeyValuePair<string, string>> headers)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new HttpHeadersMatcher(headers));
    }

    /// <summary>
    /// Matches a request by HTTP header.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="headers">The headers.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Headers(this RequestMatching builder, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new HttpHeadersMatcher(headers));
    }

    /// <summary>
    /// Matches a request by content type.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="mediaType">The content type.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching ContentType(this RequestMatching builder, string mediaType)
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
    public static RequestMatching ContentType(this RequestMatching builder, string contentType, Encoding encoding)
    {
        if (contentType is null)
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        if (encoding is null)
        {
            throw new ArgumentNullException(nameof(encoding));
        }

        var mediaType = new MediaTypeHeaderValue(contentType) { CharSet = encoding?.WebName };
        return builder.ContentType(mediaType);
    }

    /// <summary>
    /// Matches a request by media type.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="mediaType">The media type.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching ContentType(this RequestMatching builder, MediaTypeHeaderValue mediaType)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (mediaType is null)
        {
            throw new ArgumentNullException(nameof(mediaType));
        }

        return builder.With(new MediaTypeHeaderMatcher(mediaType));
    }

    /// <summary>
    /// Matches a request by form data.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="key">The form data parameter key.</param>
    /// <param name="value">The form data value.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching FormData(this RequestMatching builder, string key, string value)
    {
        return builder.FormData(new[] { new KeyValuePair<string, string>(key, value) });
    }

    /// <summary>
    /// Matches a request by form data.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="formData">The form data parameters.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching FormData(this RequestMatching builder, IEnumerable<KeyValuePair<string, string>> formData)
    {
        return builder.FormData(formData?.Select(
            d => new KeyValuePair<string, IEnumerable<string>>(
                d.Key,
                d.Value is null ? null : new[] { d.Value })
        ));
    }

    /// <summary>
    /// Matches a request by form data.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="formData">The form data parameters.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching FormData(this RequestMatching builder, NameValueCollection formData)
    {
        return builder.FormData(formData?.AsEnumerable());
    }

    /// <summary>
    /// Matches a request by form data.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="formData">The form data parameters.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching FormData(this RequestMatching builder, IEnumerable<KeyValuePair<string, IEnumerable<string>>> formData)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new FormDataMatcher(formData));
    }

    /// <summary>
    /// Matches a request by form data.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="urlEncodedFormData">The form data.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching FormData(this RequestMatching builder, string urlEncodedFormData)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrEmpty(urlEncodedFormData))
        {
            throw new ArgumentException("Specify the url encoded form data.", nameof(urlEncodedFormData));
        }

        return builder.With(new FormDataMatcher(urlEncodedFormData));
    }

    /// <summary>
    /// Matches a request by request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="content">The request content.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete(DeprecationWarnings.RequestContent)]
    public static RequestMatching Content(this RequestMatching builder, string content)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return builder.Body(content);
    }

    /// <summary>
    /// Matches a request by the request body.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="body">The expected request body.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Body(this RequestMatching builder, string body)
    {
        return builder.Body(body, ContentMatcher.DefaultEncoding);
    }

    /// <summary>
    /// Matches a request by request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="content">The request content.</param>
    /// <param name="encoding">The request content encoding.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete(DeprecationWarnings.RequestContent)]
    public static RequestMatching Content(this RequestMatching builder, string content, Encoding encoding)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return builder.Body(content, encoding);
    }

    /// <summary>
    /// Matches a request by request body.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="body">The expected request body.</param>
    /// <param name="encoding">The request content encoding.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Body(this RequestMatching builder, string body, Encoding encoding)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (body is null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        return builder.With(new ContentMatcher(body, encoding));
    }

    /// <summary>
    /// Matches a request by request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="content">The request content.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete(DeprecationWarnings.RequestContent)]
    public static RequestMatching Content(this RequestMatching builder, byte[] content)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return builder.Body(content);
    }

    /// <summary>
    /// Matches a request by request body.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="body">The expected request body.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Body(this RequestMatching builder, byte[] body)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (body is null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        return builder.With(new ContentMatcher(body));
    }

    /// <summary>
    /// Matches a request by request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="content">The request content.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete(DeprecationWarnings.RequestContent)]
    public static RequestMatching Content(this RequestMatching builder, Stream content)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return builder.Body(content);
    }

    /// <summary>
    /// Matches a request by request body.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="body">The expected request body.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Body(this RequestMatching builder, Stream body)
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
    /// Matches a request explicitly that has no request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete(DeprecationWarnings.RequestWithoutContent)]
    public static RequestMatching WithoutContent(this RequestMatching builder)
    {
        return builder.WithoutBody();
    }

    /// <summary>
    /// Matches a request explicitly that has no request body.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching WithoutBody(this RequestMatching builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new ContentMatcher());
    }

    /// <summary>
    /// Matches a request by partially matching the request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="partialContent">The request content.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete(DeprecationWarnings.RequestPartialContent)]
    public static RequestMatching PartialContent(this RequestMatching builder, string partialContent)
    {
        if (partialContent is null)
        {
            throw new ArgumentNullException(nameof(partialContent));
        }

        return builder.PartialBody(partialContent);
    }

    /// <summary>
    /// Matches a request by partially matching the request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="partialBody">The partial request body.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching PartialBody(this RequestMatching builder, string partialBody)
    {
        return builder.PartialBody(partialBody, ContentMatcher.DefaultEncoding);
    }

    /// <summary>
    /// Matches a request by partially matching the request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="partialContent">The request content.</param>
    /// <param name="encoding">The request content encoding.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete(DeprecationWarnings.RequestPartialContent)]
    public static RequestMatching PartialContent(this RequestMatching builder, string partialContent, Encoding encoding)
    {
        if (partialContent is null)
        {
            throw new ArgumentNullException(nameof(partialContent));
        }

        return builder.PartialBody(partialContent, encoding);
    }

    /// <summary>
    /// Matches a request by partially matching the request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="partialBody">The partial request body.</param>
    /// <param name="encoding">The request content encoding.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching PartialBody(this RequestMatching builder, string partialBody, Encoding encoding)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (partialBody is null)
        {
            throw new ArgumentNullException(nameof(partialBody));
        }

        return builder.With(new PartialContentMatcher(partialBody, encoding));
    }

    /// <summary>
    /// Matches a request by partially matching the request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="partialContent">The request content.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete(DeprecationWarnings.RequestPartialContent)]
    public static RequestMatching PartialContent(this RequestMatching builder, byte[] partialContent)
    {
        if (partialContent is null)
        {
            throw new ArgumentNullException(nameof(partialContent));
        }

        return builder.PartialBody(partialContent);
    }

    /// <summary>
    /// Matches a request by partially matching the request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="partialBody">The partial request body.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching PartialBody(this RequestMatching builder, byte[] partialBody)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (partialBody is null)
        {
            throw new ArgumentNullException(nameof(partialBody));
        }

        return builder.With(new PartialContentMatcher(partialBody));
    }

    /// <summary>
    /// Matches a request by partially matching the request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="partialContent">The request content.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete(DeprecationWarnings.RequestPartialContent)]
    public static RequestMatching PartialContent(this RequestMatching builder, Stream partialContent)
    {
        if (partialContent is null)
        {
            throw new ArgumentNullException(nameof(partialContent));
        }

        return builder.PartialBody(partialContent);
    }

    /// <summary>
    /// Matches a request by partially matching the request content.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="partialBody">The request content.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching PartialBody(this RequestMatching builder, Stream partialBody)
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
    public static RequestMatching Any(this RequestMatching builder, Action<RequestMatching> anyBuilder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (anyBuilder is null)
        {
            throw new ArgumentNullException(nameof(anyBuilder));
        }

        var anyRequestMatching = new AnyRequestMatching();
        anyBuilder(anyRequestMatching);
        return builder.With(new AnyMatcher(anyRequestMatching.Build()));
    }

    /// <summary>
    /// Matches a request using a custom expression.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="expression">The expression.</param>
    /// <returns>The request matching builder instance.</returns>
    [Obsolete("Replaced by " + nameof(Where) + ".")]
    public static RequestMatching When(this RequestMatching builder, Expression<Func<HttpRequestMessage, bool>> expression)
    {
        return builder.Where(expression);
    }

    /// <summary>
    /// Matches a request using a custom expression.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="expression">The expression.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Where(this RequestMatching builder, Expression<Func<HttpRequestMessage, bool>> expression)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new ExpressionMatcher(expression));
    }

    /// <summary>
    /// Matches a request by matching the request message version.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="version">The message version.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Version(this RequestMatching builder, string version)
    {
        return builder.Version(version is null ? null : System.Version.Parse(version));
    }

    /// <summary>
    /// Matches a request by matching the request message version.
    /// </summary>
    /// <param name="builder">The request matching builder instance.</param>
    /// <param name="version">The message version.</param>
    /// <returns>The request matching builder instance.</returns>
    public static RequestMatching Version(this RequestMatching builder, Version version)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.With(new VersionMatcher(version));
    }
}
