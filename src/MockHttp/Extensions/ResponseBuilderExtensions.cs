#nullable enable
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using MockHttp.Http;
using MockHttp.IO;
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
    /// <param name="reasonPhrase">The reason phrase which typically is sent by servers together with the status code.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="statusCode" /> is less than 100.</exception>
    public static IWithStatusCodeResult StatusCode(this IWithStatusCode builder, int statusCode, string? reasonPhrase = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.StatusCode((HttpStatusCode)statusCode, reasonPhrase);
    }

    /// <summary>
    /// Sets the <see cref="HttpContent" /> for the response.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="content">The HTTP content.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="content" /> is <see langword="null" />.</exception>
    public static IWithContentResult Body(this IWithContent builder, HttpContent content)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return builder.Body(() => Task.FromResult(content));
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
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return builder.Body(() => Task.FromResult<HttpContent>(new StringContent(content, encoding)));
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
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

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
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (offset < 0 || offset > content.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        if (count < 0 || count > content.Length - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        return builder.Body(() => Task.FromResult<HttpContent>(
            new ByteArrayContent(content, offset, count) { Headers = { ContentType = new MediaTypeHeaderValue(MediaTypes.OctetStream) } })
        );
    }

    /// <summary>
    /// Sets the stream content for the response.
    /// <para>This overload is not thread safe. Do not use when requests are (expected to be) served in parallel.</para>
    /// <para>This overload should not be used with large (> ~1 megabyte) non-seekable streams, due to internal buffering.</para>
    /// <para>This overload cannot be used with a <see cref="RateLimitedStream" /> if the underlying stream is not seekable.</para>
    /// <para>If the above cases are true, it is recommend to use the overload that accepts <c>Func&lt;Stream&gt;</c>.</para>
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="content">The stream content.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="content" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="content" /> stream does not support reading.</exception>
    public static IWithContentResult Body(this IWithContent builder, Stream content)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (!content.CanRead)
        {
            throw new ArgumentException("Cannot read from stream.", nameof(content));
        }

        if (content.CanSeek)
        {
            // Stream is reusable, delegate to Func<Stream>.
            // Note: this is not thread safe!
            long startPos = content.Position;
            return builder.Body(() =>
            {
                content.Position = startPos;
                return content;
            });
        }

        if (content is RateLimitedStream)
        {
            throw new InvalidOperationException("Cannot use a rate limited stream that is not seekable. Use the overload accepting a stream factory instead.");
        }

        // Because stream is not seekable, we must clone so it can be rewound and served more than once.
        // This could be an issue with very big streams.
        // Should we throw if greater than a certain size and force use of Func<Stream> instead?
        byte[] buffer;
        using (var ms = new MemoryStream())
        {
            content.CopyTo(ms);
            buffer = ms.ToArray();
        }

        return builder.Body(buffer);
    }

    /// <summary>
    /// Sets the stream content for the response using a factory returning a new <see cref="Stream" /> on each invocation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="contentFactory">The factory returning a new <see cref="Stream" /> on each invocation containing the binary content.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="contentFactory" /> is <see langword="null" />.</exception>
    public static IWithContentResult Body(this IWithContent builder, Func<Stream> contentFactory)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (contentFactory is null)
        {
            throw new ArgumentNullException(nameof(contentFactory));
        }

        return builder.Body(() =>
        {
            Stream stream = contentFactory();
            if (!stream.CanRead)
            {
                throw new InvalidOperationException("Cannot read from stream.");
            }

            return Task.FromResult<HttpContent>(
                new StreamContent(stream) { Headers = { ContentType = new MediaTypeHeaderValue(MediaTypes.OctetStream) } }
            );
        });
    }

    /// <summary>
    /// Sets the media type for the response. Will be ignored if no content is set.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="mediaType">The media type.</param>
    /// <param name="encoding">The optional encoding.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="mediaType" /> is <see langword="null" />.</exception>
    /// <exception cref="FormatException">Throw if the <paramref name="mediaType" /> is invalid.</exception>
    public static IWithHeadersResult ContentType(this IWithContentType builder, string mediaType, Encoding? encoding = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (mediaType is null)
        {
            throw new ArgumentNullException(nameof(mediaType));
        }

        var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
        if (encoding is not null)
        {
            mediaTypeHeaderValue.CharSet = encoding.WebName;
        }

        return builder.ContentType(mediaTypeHeaderValue);
    }

    /// <summary>
    /// Adds a HTTP header value.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value or values.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="name" /> is <see langword="null" />.</exception>
    public static IWithHeadersResult Header<T>(this IWithHeaders builder, string name, params T[] value)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        return builder.Header(new KeyValuePair<string, IEnumerable<T>>(name, value ?? Array.Empty<T>()));
    }

    /// <summary>
    /// Adds a HTTP header value.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="header">The header.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is <see langword="null" />.</exception>
    public static IWithHeadersResult Header<T>(this IWithHeaders builder, KeyValuePair<string, T> header)
    {
        return builder.Header(new KeyValuePair<string, IEnumerable<T>>(header.Key, new [] { header.Value }));
    }

    /// <summary>
    /// Adds a HTTP header value.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="header">The header.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is <see langword="null" />.</exception>
    public static IWithHeadersResult Header<T>(this IWithHeaders builder, KeyValuePair<string, IEnumerable<T>> header)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.Headers(new HttpHeadersCollection { { header.Key, header.Value.Select(ConvertToString) } }!);
    }

    /// <summary>
    /// Specifies to throw a <see cref="TaskCanceledException" /> after a specified amount of time, simulating a HTTP client request timeout.
    /// <para>Note: the response is short-circuited when running outside of the HTTP mock server. To simulate server timeouts, use <c>.StatusCode(HttpStatusCode.RequestTimeout)</c> or <see cref="ServerTimeout"/> instead.</para>
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="timeoutAfter">The time after which the timeout occurs. If <see langword="null" />, the <see cref="TaskCanceledException" /> is thrown immediately.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is <see langword="null" />.</exception>
    public static IWithResponse ClientTimeout(this IResponseBuilder builder, TimeSpan? timeoutAfter = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (timeoutAfter < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutAfter), "The timeout cannot be less than 00:00:00.");
        }

        builder.Behaviors.Replace(new TimeoutBehavior(timeoutAfter ?? TimeSpan.Zero));
        return builder;
    }

    /// <summary>
    /// Specifies to timeout the request after a specified amount of time with a <see cref="HttpStatusCode.RequestTimeout"/>, simulating a HTTP request timeout.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="timeoutAfter">The time after which the timeout occurs. If <see langword="null" />, the timeout occurs immediately (effectively this would then be the same as using <c>.StatusCode(HttpStatusCode.RequestTimeout)</c>.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> is <see langword="null" />.</exception>
    public static IWithStatusCodeResult ServerTimeout(this IResponseBuilder builder, TimeSpan? timeoutAfter = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (timeoutAfter < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutAfter), "The timeout cannot be less than 00:00:00.");
        }

        IWithStatusCodeResult statusCodeResult = builder.StatusCode(HttpStatusCode.RequestTimeout);
        if (!timeoutAfter.HasValue || timeoutAfter == TimeSpan.Zero)
        {
            return statusCodeResult;
        }

        return (IWithStatusCodeResult)statusCodeResult.Latency(NetworkLatency.Around(timeoutAfter.Value));
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

        if (latency is null)
        {
            throw new ArgumentNullException(nameof(latency));
        }

        int existingIndex = builder.Behaviors.IndexOf(typeof(NetworkLatencyBehavior));
        if (existingIndex == -1)
        {
            builder.Behaviors.Insert(0, new NetworkLatencyBehavior(latency));
        }
        else
        {
            builder.Behaviors[existingIndex] = new NetworkLatencyBehavior(latency);
        }
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
        if (latency is null)
        {
            throw new ArgumentNullException(nameof(latency));
        }

        return builder.Latency(latency());
    }

    private static string? ConvertToString<T>(T v)
    {
        switch (v)
        {
            case string str:
                return str;

            case DateTime dt:
                return dt.ToString("R", DateTimeFormatInfo.InvariantInfo);

            case DateTimeOffset dtOffset:
                return dtOffset.ToString("R", DateTimeFormatInfo.InvariantInfo);

            case null:
                return null;

            default:
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                return converter.ConvertToString(null!, CultureInfo.InvariantCulture, v);
            }
        }
    }

}
#nullable restore
