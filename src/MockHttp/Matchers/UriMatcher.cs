using System.Runtime.CompilerServices;
using MockHttp.Matchers.Patterns;
using MockHttp.Responses;

namespace MockHttp.Matchers;

/// <summary>
/// Matches a request by the URI.
/// </summary>
internal class UriMatcher : HttpRequestMatcher
{
    private readonly IPatternMatcher<Uri> _patternMatcher;
    private readonly string _name;
    private readonly string _patternDescription;

    /// <summary>
    /// Initializes a new instance of the <see cref="UriMatcher" /> class.
    /// </summary>
    /// <param name="patternMatcher">A matcher implementation that validates the URI.</param>
    /// <param name="patternDescription">A description of the pattern.</param>
    /// <param name="name">The name of this matcher.</param>
    /// <exception cref="ArgumentNullException">Thrown when a required argument is <see langword="null" />.</exception>
    internal UriMatcher
    (
        IPatternMatcher<Uri> patternMatcher,
        string patternDescription,
        [CallerMemberName] string? name = null
    )
    {
        _patternMatcher = patternMatcher ?? throw new ArgumentNullException(nameof(patternMatcher));
        _patternDescription = patternDescription ?? throw new ArgumentNullException(nameof(patternDescription));

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
        return uri is not null && _patternMatcher.IsMatch(uri);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{_name}: '{_patternDescription}'";
    }
}
