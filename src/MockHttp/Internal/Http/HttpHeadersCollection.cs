using System.Net.Http.Headers;

namespace MockHttp.Http;

internal sealed class HttpHeadersCollection : HttpHeaders
{
    private static readonly char[] HeaderKeyValueSeparator = new[] { ':' };
    private static readonly char[] HeaderValueSeparator = new[] { ',' };

    public HttpHeadersCollection()
    {
    }

    public HttpHeadersCollection(IEnumerable<KeyValuePair<string, IEnumerable<string?>>> headers)
    {
        foreach (KeyValuePair<string, IEnumerable<string?>> header in headers)
        {
            Add(header.Key, header.Value);
        }
    }

    public static HttpHeaders Parse(string headers)
    {
        if (headers is null)
        {
            throw new ArgumentNullException(nameof(headers));
        }

        var httpHeaders = new HttpHeadersCollection();
        if (headers.Length == 0)
        {
            return httpHeaders;
        }

        using var sr = new StringReader(headers);
        while (true)
        {
            string? header = sr.ReadLine();
            if (header is null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }

            string[] hvp = header.Split(HeaderKeyValueSeparator, 2, StringSplitOptions.None);

            string fieldName = hvp.Length > 0 ? hvp[0] : string.Empty;
            string? fieldValue = hvp.Length > 1 ? hvp[1] : null;
            httpHeaders.Add(fieldName, ParseHttpHeaderValue(fieldValue));
        }

        return httpHeaders;
    }

    internal static IEnumerable<string> ParseHttpHeaderValue(string? headerValue)
    {
        if (headerValue is null)
        {
            throw new ArgumentNullException(nameof(headerValue), "The value cannot be null or empty.");
        }

        return headerValue
            .Split(HeaderValueSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(v => v.Trim())
            .Where(v => v.Length > 0)
            .ToArray();
    }
}
