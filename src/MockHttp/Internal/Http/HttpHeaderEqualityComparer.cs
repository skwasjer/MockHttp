using System.ComponentModel;
using MockHttp.Patterns;

namespace MockHttp.Http;

internal sealed class HttpHeaderEqualityComparer : IEqualityComparer<KeyValuePair<string, IEnumerable<string>>>
{
    private readonly HttpHeaderMatchType? _matchType;
    private readonly Matches? _valueMatches;

    public HttpHeaderEqualityComparer(HttpHeaderMatchType matchType)
    {
#if NET8_0_OR_GREATER
        if (!Enum.IsDefined(matchType))
#else
        if (!Enum.IsDefined(typeof(HttpHeaderMatchType), matchType))
#endif
        {
            throw new InvalidEnumArgumentException(nameof(matchType), (int)matchType, typeof(HttpHeaderMatchType));
        }

        _matchType = matchType;
    }

    public HttpHeaderEqualityComparer(Matches valueMatches)
    {
        _valueMatches = valueMatches;
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
                                return (!_valueMatches.HasValue && headerValues.Contains(xValue))
                                    || (_valueMatches.HasValue && headerValues.Any(_valueMatches.Value.IsMatch));
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

#pragma warning disable CA1065 // Justification: not used with dictionaries/hash sets.
    public int GetHashCode(KeyValuePair<string, IEnumerable<string>> obj)
    {
        throw new NotImplementedException();
    }
#pragma warning restore CA1065
}
