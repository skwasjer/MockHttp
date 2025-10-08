using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MockHttp.Http;

[DebuggerDisplay("{ToString(),nq}")]
internal sealed class QueryString : Dictionary<string, IEnumerable<string>>
{
    private static readonly UriKind DotNetRelativeOrAbsolute = Type.GetType("Mono.Runtime") == null ? UriKind.RelativeOrAbsolute : (UriKind)300;

    private static readonly Uri UnknownBaseUri = new("https://0.0.0.0");

    private const char TokenQuestionMark = '?';
    private const char TokenTerminator = '#';

    public QueryString()
    {
    }

    public QueryString(IEnumerable<KeyValuePair<string, string>> values)
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        : this(values?.Select(v => new KeyValuePair<string, IEnumerable<string>>(v.Key, new[] { v.Value }))!)
    {
    }

    public QueryString(IEnumerable<KeyValuePair<string, IEnumerable<string>>> values)
        : this()
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        foreach (KeyValuePair<string, IEnumerable<string>> kvp in values)
        {
            InternalAdd(kvp.Key, kvp.Value);
        }
    }

    // TODO: override Add, because empty string for key is possible but not allowed. Even though we checked here, still possible to call Add externally.
    private void InternalAdd(string key, IEnumerable<string?>? values)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new FormatException("Key can not be null or empty.");
        }

        // Accept null values enumerable, but then use empty list.
        // Null values in values enumerable are filtered out.
        Add(key, (values?.Where(v => v is not null).ToList() ?? new List<string?>())!);
    }

    public override string ToString()
    {
        return Count == 0 ? string.Empty : "?" + DataEscapingHelper.Format(this);
    }

    public static QueryString Parse(string queryString)
    {
        if (queryString is null)
        {
            throw new ArgumentNullException(nameof(queryString));
        }

        if (!TryGetQueryFromUri(queryString, out string? qs))
        {
            qs = queryString;

            // If query string contains terminator, strip it off.
            qs = qs.Split(TokenTerminator)[0];
        }

        // If begins with question mark, strip it off.
        qs = qs.TrimStart(TokenQuestionMark);

        return new QueryString(DataEscapingHelper.Parse(qs));
    }

    private static bool TryGetQueryFromUri(string uri, [NotNullWhen(true)] out string? queryString)
    {
#if NETSTANDARD2_1 || NET8_0_OR_GREATER
        if (uri.Contains(TokenQuestionMark, StringComparison.InvariantCultureIgnoreCase)
#else
        if (uri.Contains(TokenQuestionMark.ToString(CultureInfo.InvariantCulture))
#endif
            && Uri.TryCreate(uri, DotNetRelativeOrAbsolute, out Uri? u))
        {
            if (!u.IsAbsoluteUri)
            {
                u = new Uri(UnknownBaseUri, uri);
            }

            queryString = u.Query;
            return true;
        }

        queryString = null;
        return false;
    }
}
