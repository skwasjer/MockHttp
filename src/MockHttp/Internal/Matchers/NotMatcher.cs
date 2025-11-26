namespace MockHttp.Matchers;

/// <summary>
/// Matcher that inverts the final result of matching a set of inner matchers.
/// </summary>
internal sealed class NotMatcher : IAsyncHttpRequestMatcher
{
    private readonly IAsyncHttpRequestMatcher _matcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotMatcher" /> class using specified <paramref name="matcher" />.
    /// </summary>
    /// <param name="matcher">A matcher for which the result is inverted.</param>
    public NotMatcher(IAsyncHttpRequestMatcher matcher)
    {
        _matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
    }

    /// <inheritdoc />
    public Task<bool> IsMatchAsync(MockHttpRequestContext requestContext)
    {
        if (requestContext is null)
        {
            throw new ArgumentNullException(nameof(requestContext));
        }

        return InternalIsMatchAsync(requestContext);

        async Task<bool> InternalIsMatchAsync(MockHttpRequestContext mockHttpRequestContext)
        {
            return !await _matcher.IsMatchAsync(mockHttpRequestContext).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public bool IsExclusive => false;

    /// <inheritdoc />
    public override string ToString()
    {
        return "Not " + _matcher;
    }
}
