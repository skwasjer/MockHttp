using System.ComponentModel;
using MockHttp.Matchers.Patterns;

namespace MockHttp.Http;

internal enum HttpHeaderMatchType
{
    /// <summary>
    /// Header and value(s) match exactly.
    /// </summary>
    Exact,
    /// <summary>
    /// Header name matches, values are ignored.
    /// </summary>
    HeaderNameOnly,
    /// <summary>
    /// Header name matches and all values of left comparand must be in right.
    /// </summary>
    HeaderNameAndPartialValues
}

internal sealed class HttpHeaderEqualityComparer : IEqualityComparer<KeyValuePair<string, IEnumerable<string>>>
{
    private readonly HttpHeaderMatchType? _matchType;
    private readonly IPatternMatcher<string>? _valuePatternMatcher;

    public HttpHeaderEqualityComparer(HttpHeaderMatchType matchType)
    {
        if (!Enum.IsDefined(typeof(HttpHeaderMatchType), matchType))
        {
            throw new InvalidEnumArgumentException(nameof(matchType), (int)matchType, typeof(HttpHeaderMatchType));
        }

        _matchType = matchType;
    }

    public HttpHeaderEqualityComparer(IPatternMatcher<string> valuePatternMatcher)
    {
        _valuePatternMatcher = valuePatternMatcher ?? throw new ArgumentNullException(nameof(valuePatternMatcher));
    }

    public bool Equals(KeyValuePair<string, IEnumerable<string>> x, KeyValuePair<string, IEnumerable<string>> y)
    {
        if (x.Key != y.Key)
        {
            return false;
        }

        switch (_matchType)
        {
            case HttpHeaderMatchType.HeaderNameOnly:
                return true;
            case HttpHeaderMatchType.Exact:
                return x.Value.SelectMany(HttpHeadersCollection.ParseHttpHeaderValue)
                    .SequenceEqual(y.Value.SelectMany(HttpHeadersCollection.ParseHttpHeaderValue));
            case HttpHeaderMatchType.HeaderNameAndPartialValues:
            case null:
            {
                if (y.Value.Any(
                        yValue => x.Value
                            .SelectMany(HttpHeadersCollection.ParseHttpHeaderValue)
                            .All(xValue =>
                            {
                                string[] headerValues = HttpHeadersCollection.ParseHttpHeaderValue(yValue).ToArray();
                                return _valuePatternMatcher is null && headerValues.Contains(xValue)
                                 || (_valuePatternMatcher is not null && headerValues.Any(_valuePatternMatcher.IsMatch));
                            })
                    ))
                {
                    return true;
                }

                return !x.Value.Any();
            }
            default:
                return false;
        }
    }

    public int GetHashCode(KeyValuePair<string, IEnumerable<string>> obj)
    {
        throw new NotImplementedException();
    }
}
