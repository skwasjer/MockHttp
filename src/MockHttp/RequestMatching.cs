using System.Collections.ObjectModel;
using System.ComponentModel;
using MockHttp.Language;
using MockHttp.Matchers;
using MockHttp.Request;

namespace MockHttp;

/// <summary>
/// A builder to configure request matchers.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal class RequestMatching : IRequestMatching, IFluentInterface
{
    private readonly List<IAsyncHttpRequestMatcher> _matchers = [];
    private RequestMatching? _not;

    /// <inheritdoc />
    public IRequestMatching Not => _not ??= new InvertRequestMatching(this);

    /// <inheritdoc />
    public virtual IRequestMatching Add(IAsyncHttpRequestMatcher matcher)
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

    /// <inheritdoc />
    public IReadOnlyCollection<IAsyncHttpRequestMatcher> Build()
    {
        return new ReadOnlyCollection<IAsyncHttpRequestMatcher>(_matchers.ToArray());
    }
}
