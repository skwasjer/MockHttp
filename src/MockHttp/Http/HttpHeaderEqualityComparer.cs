using System.ComponentModel;
using MockHttp.Patterns;

namespace MockHttp.Http;

internal sealed class HttpHeaderEqualityComparer : IEqualityComparer<KeyValuePair<string, IEnumerable<string>>>
{
    private readonly HttpHeaderMatchType? _matchType;
    private readonly Pattern? _valuePatternMatcher;

    public HttpHeaderEqualityComparer(HttpHeaderMatchType matchType)
    {
        if (!Enum.IsDefined(typeof(HttpHeaderMatchType), matchType))
        {
            throw new InvalidEnumArgumentException(nameof(matchType), (int)matchType, typeof(HttpHeaderMatchType));
        }

        _matchType = matchType;
    }

    public HttpHeaderEqualityComparer(Pattern valuePatternMatcher)
    {
        _valuePatternMatcher = valuePatternMatcher;
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
                                return (!_valuePatternMatcher.HasValue && headerValues.Contains(xValue))
                                    || (_valuePatternMatcher.HasValue && headerValues.Any(_valuePatternMatcher.Value.IsMatch));
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
