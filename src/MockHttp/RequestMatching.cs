using System.Collections.ObjectModel;
using System.ComponentModel;
using MockHttp.Matchers;
using MockHttp.Request;

namespace MockHttp;

/// <summary>
/// A builder to configure request matchers.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class RequestMatching : IFluentInterface
{
    private readonly List<IAsyncHttpRequestMatcher> _matchers = new();
    private RequestMatching? _not;

    internal RequestMatching()
    {
    }

    /// <summary>
    /// Gets a request matcher that will not match any constraint configured on it.
    /// </summary>
    public RequestMatching Not => _not ??= new InvertRequestMatching(this);

    /// <summary>
    /// Adds a matcher.
    /// </summary>
    /// <param name="matcher">The matcher instance.</param>
    /// <returns>The request matching builder.</returns>
    public RequestMatching With(IAsyncHttpRequestMatcher matcher)
    {
        return RegisterMatcher(matcher);
    }

    /// <summary>
    /// Adds a matcher.
    /// </summary>
    /// <param name="matcher">The matcher instance.</param>
    /// <returns>The request matching builder.</returns>
    // ReSharper disable once MemberCanBeProtected.Global
    protected internal virtual RequestMatching RegisterMatcher(IAsyncHttpRequestMatcher matcher)
    {
        if (matcher is null)
        {
            throw new ArgumentNullException(nameof(matcher));
        }

        if (_matchers.Contains(matcher))
        {
            return this;
        }

        ValidateMatcher(matcher);

        _matchers.Add(matcher);
        return this;
    }

    /// <summary>
    /// </summary>
    // ReSharper disable once MemberCanBeProtected.Global
    protected internal virtual void ValidateMatcher(IAsyncHttpRequestMatcher matcher)
    {
        if (matcher is null)
        {
            throw new ArgumentNullException(nameof(matcher));
        }

        var sameTypeMatchers = _matchers
            .Where(m => m.GetType() == matcher.GetType())
            .ToList();

        if ((matcher.IsExclusive && sameTypeMatchers.Count > 0) || (!matcher.IsExclusive && sameTypeMatchers.Any(m => m.IsExclusive)))
        {
            throw new InvalidOperationException($"Cannot add matcher, another matcher of type '{matcher.GetType().FullName}' already is configured.");
        }
    }

    internal IReadOnlyCollection<IAsyncHttpRequestMatcher> Build()
    {
        return new ReadOnlyCollection<IAsyncHttpRequestMatcher>(_matchers.ToArray());
    }
}
