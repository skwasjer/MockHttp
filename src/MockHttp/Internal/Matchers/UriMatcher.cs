using System.Runtime.CompilerServices;
using MockHttp.Patterns;

namespace MockHttp.Matchers;

/// <summary>
/// Matches a request by the URI.
/// </summary>
internal sealed class UriMatcher : HttpRequestMatcher
{
    private readonly string _name;
    private readonly Matches _matches;
    private readonly string _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="UriMatcher" /> class.
    /// </summary>
    /// <param name="matches">The string matcher to use to match the URI.</param>
    /// <param name="name">The name of this matcher.</param>
    /// <exception cref="ArgumentNullException">Thrown when a required argument is <see langword="null" />.</exception>
    internal UriMatcher(
        Matches matches,
        [CallerMemberName] string? name = null
    )
    {
        _matches = matches;
        _description = matches.Value;

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

        return _matches.IsMatch(uri.ToString());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{_name}: '{_description}'";
    }
}
