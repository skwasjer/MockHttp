using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MockHttp.Patterns;
using MockHttp.Responses;

namespace MockHttp.Matchers;

/// <summary>
/// Matches a request by the URI.
/// </summary>
internal sealed class UriMatcher : HttpRequestMatcher
{
    private readonly string _name;
    private readonly Pattern _pattern;
    private readonly string _patternDescription;
    private readonly Func<Uri, string>? _selectorFn;

    /// <summary>
    /// Initializes a new instance of the <see cref="UriMatcher" /> class.
    /// </summary>
    /// <param name="pattern">The pattern to match.</param>
    /// <param name="selector">An expression to extract the part of the URI to match against. If <see langword="null" />, uses the complete URI.</param>
    /// <param name="name">The name of this matcher.</param>
    /// <exception cref="ArgumentNullException">Thrown when a required argument is <see langword="null" />.</exception>
    internal UriMatcher
    (
        Pattern pattern,
        Expression<Func<Uri, string>>? selector = null,
        [CallerMemberName] string? name = null
    )
    {
        _pattern = pattern;
        _selectorFn = selector?.Compile();
        _patternDescription = pattern.Value;

        name ??= GetType().Name;
        if (name.EndsWith("Matcher", StringComparison.Ordinal))
        {
            name = name.Remove(name.Length - "Matcher".Length);
        }

        _name = name;
    }

    /// <inheritdoc />
    public override bool IsMatch(MockHttpRequestContext requestContext)
    {
        if (requestContext is null)
        {
            throw new ArgumentNullException(nameof(requestContext));
        }

        Uri? uri = requestContext.Request.RequestUri;
        if (uri is null)
        {
            return false;
        }

        string value = _selectorFn?.Invoke(uri) ?? uri.ToString();
        return _pattern.IsMatch(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{_name}: '{_patternDescription}'";
    }
}
