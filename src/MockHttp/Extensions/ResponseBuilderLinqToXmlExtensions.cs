using System.Net.Http.Headers;
using System.Xml;
using System.Xml.Linq;
using MockHttp.Http;
using MockHttp.Language.Flow.Response;
using MockHttp.Language.Response;

namespace MockHttp;

/// <summary>
/// Response builder extensions for LINQ to XML.
/// </summary>
public static class ResponseBuilderLinqToXmlExtensions
{
    /// <summary>
    /// Sets the response body to the specified <paramref name="xmlContent" /> with default content-type <c>application/xml</c>. If an XML declaration is present, its encoding will be used; otherwise the encoding from <paramref name="settings" /> is used (default is UTF-8).
    /// <para>To use a different content type use the <see cref="ResponseBuilderExtensions.ContentType" /> extension following this.</para>
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="xmlContent">The XML element or document to write to the response body.</param>
    /// <param name="settings">The optional XML writer settings.</param>
    /// <returns>The builder to continue chaining additional behaviors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="xmlContent" /> is <see langword="null" />.</exception>
    public static IWithContentResult XmlBody(this IWithContent builder, XContainer xmlContent, XmlWriterSettings? settings = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (xmlContent is null)
        {
            throw new ArgumentNullException(nameof(xmlContent));
        }

        settings ??= new XmlWriterSettings();
        // Ensure the stream is not closed/disposed on disposal of the XML writer.
        // It will be disposed by HttpClient instead.
        settings.CloseOutput = false;
        settings.Async = true;

        return builder
            .Body(async ct =>
            {
                var stream = new MemoryStream();
#if NET6_0_OR_GREATER
                var xmlWriter = XmlWriter.Create(stream, settings);
                await using (xmlWriter.ConfigureAwait(false))
                {
                    return await WriteAsync(xmlWriter).ConfigureAwait(false);
                }
#else
                using var xmlWriter = XmlWriter.Create(stream, settings);
                return await WriteAsync(xmlWriter).ConfigureAwait(false);
#endif

                async Task<HttpContent> WriteAsync(XmlWriter writer)
                {
#if NETSTANDARD2_1 || NET6_0_OR_GREATER
                    await xmlContent.WriteToAsync(writer, ct).ConfigureAwait(false);
#else
                    xmlContent.WriteTo(writer);
#endif
                    await writer.FlushAsync().ConfigureAwait(false);
                    stream.Position = 0;
                    return new StreamContent(stream) { Headers = { ContentType = new MediaTypeHeaderValue(MediaTypes.Xml) { CharSet = settings.Encoding.WebName } } };
                }
            });
    }
}
