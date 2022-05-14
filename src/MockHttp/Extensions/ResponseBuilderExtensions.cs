﻿#nullable enable
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using MockHttp.Http;
using MockHttp.Language.Flow.Response;
using MockHttp.Language.Response;
using MockHttp.Responses;

namespace MockHttp;

/// <summary>
/// Response builder extensions.
/// </summary>
public static class ResponseBuilderExtensions
{
    /// <summary>
    /// Sets the status code for the response.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="statusCode">The status code to return with the response.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="statusCode" /> is less than 100.</exception>
    public static IWithStatusCodeResult StatusCode(this IWithStatusCode builder, int statusCode)
    {
        return builder.StatusCode((HttpStatusCode)statusCode);
    }

    /// <summary>
    /// Sets the plain text content for the response.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="content">The plain text content.</param>
    /// <param name="encoding">The optional encoding to use when encoding the text.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="content" /> is <see langword="null" />.</exception>
    public static IWithContentResult Body(this IWithContent builder, string content, Encoding? encoding = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return builder.Body(_ => Task.FromResult<HttpContent>(new StringContent(content, encoding)));
    }

    /// <summary>
    /// Sets the binary content for the response.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="content">The binary content.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="content" /> is <see langword="null" />.</exception>
    public static IWithContentResult Body(this IWithContent builder, byte[] content)
    {
        return builder.Body(content, 0, content.Length);
    }

    /// <summary>
    /// Sets the binary content for the response.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="content">The binary content.</param>
    /// <param name="offset">The offset in the array.</param>
    /// <param name="count">The number of bytes to return, starting from the offset.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="content" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="offset" /> or <paramref name="count" /> exceeds beyond end of array.</exception>
    public static IWithContentResult Body(this IWithContent builder, byte[] content, int offset, int count)
    {
        if (offset < 0 || offset > content.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        if (count < 0 || count > content.Length - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        return builder.Body(_ => Task.FromResult<HttpContent>(new ByteArrayContent(content, offset, count)));
    }

    /// <summary>
    /// Sets the binary content for the response.
    /// <para>It is recommend to use the overload that accepts Func&lt;Stream&gt; with very large streams due to internal in-memory buffering.</para>
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="content">The binary content.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="content" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="content" /> stream does not support reading.</exception>
    public static IWithContentResult Body(this IWithContent builder, Stream content)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (!content.CanRead)
        {
            throw new ArgumentException("Cannot read from stream.", nameof(content));
        }

        // We must copy byte buffer because streams are not thread safe nor are reset to offset 0 after first use.
        byte[] buffer;
        using (var ms = new MemoryStream())
        {
            content.CopyTo(ms);
            // This could be an issue with very big streams.
            // Should we throw if greater than a certain size and force use of Func<Stream> instead?
            buffer = ms.ToArray();
        }

        return builder.Body(buffer);
    }

    /// <summary>
    /// Sets the binary content for the response using a factory returning a new <see cref="Stream" /> on each invocation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="streamFactory">The factory returning a new <see cref="Stream" /> on each invocation containing the binary content.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="streamFactory" /> is <see langword="null" />.</exception>
    public static IWithContentResult Body(this IWithContent builder, Func<Stream> streamFactory)
    {
        if (streamFactory is null)
        {
            throw new ArgumentNullException(nameof(streamFactory));
        }

        return builder.Body(_ =>
        {
            Stream stream = streamFactory();
            if (!stream.CanRead)
            {
                throw new ArgumentException("Cannot read from stream.", nameof(stream));
            }

            return Task.FromResult<HttpContent>(new StreamContent(stream));
        });
    }

    /// <summary>
    /// Sets the content type for the response. Will be ignored if no content is set.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="encoding">The optional encoding.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="contentType" /> is <see langword="null" />.</exception>
    public static IWithHeadersResult ContentType(this IWithContentType builder, string contentType, Encoding? encoding = null)
    {
        if (contentType is null)
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        var mediaType = MediaTypeHeaderValue.Parse(contentType);
        if (encoding is not null)
        {
            mediaType.CharSet = encoding.WebName;
        }

        return builder.ContentType(mediaType);
    }

    /// <summary>
    /// Adds a HTTP header value.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="name" /> is <see langword="null" />.</exception>
    public static IWithHeadersResult Header(this IWithHeaders builder, string name, string value)
    {
        return builder.Header(name, new[] { value });
    }

    /// <summary>
    /// Adds HTTP header with values.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="name">The header name.</param>
    /// <param name="values">The header values.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="name" /> is <see langword="null" />.</exception>
    public static IWithHeadersResult Header(this IWithHeaders builder, string name, IEnumerable<string> values)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        return builder.Headers(new HttpHeadersCollection { { name, values } });
    }

    /// <summary>
    /// Specifies to throw a <see cref="TaskCanceledException" /> simulating a HTTP client request timeout.
    /// <para>Note: the response is short-circuited when running outside of the HTTP mock server. To simulate server timeouts, use <code>.StatusCode(HttpStatusCode.RequestTimeout)</code> instead.</para>
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is <see langword="null" />.</exception>
    public static IResponseBuilder TimesOut(this IResponseBuilder builder)
    {
        return builder.TimesOutAfter(TimeSpan.Zero);
    }

    /// <summary>
    /// Specifies to throw a <see cref="TaskCanceledException" /> after a specified amount of time, simulating a HTTP client request timeout.
    /// <para>Note: the response is short-circuited when running outside of the HTTP mock server. To simulate server timeouts, use <c>.StatusCode(HttpStatusCode.RequestTimeout)</c> instead.</para>
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="timeoutAfter">The time after which the timeout occurs.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is <see langword="null" />.</exception>
    public static IResponseBuilder TimesOutAfter(this IResponseBuilder builder, TimeSpan timeoutAfter)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Behaviors.Replace(new TimeoutBehavior(timeoutAfter));
        return builder;
    }

    /// <summary>
    /// Adds artificial (simulated) network latency to the request/response.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="latency">The network latency to simulate.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="latency" /> is <see langword="null" />.</exception>
    public static IWithResponse Latency(this IWithResponse builder, NetworkLatency latency)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Behaviors.Replace(new NetworkLatencyBehavior(latency));
        return builder;
    }

    /// <summary>
    /// Adds artificial (simulated) network latency to the request/response.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="latency">The network latency to simulate.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="latency" /> is <see langword="null" />.</exception>
    public static IWithResponse Latency(this IWithResponse builder, Func<NetworkLatency> latency)
    {
        return builder.Latency(latency());
    }
}
#nullable restore
