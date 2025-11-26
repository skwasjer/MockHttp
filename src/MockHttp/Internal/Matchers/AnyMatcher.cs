using System.Text;
using MockHttp.Extensions;

namespace MockHttp.Matchers;

/// <summary>
/// Matches a request by verifying it against a list of constraints, for which at least one has to match the request.
/// </summary>
internal sealed class AnyMatcher : IAsyncHttpRequestMatcher
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnyMatcher" /> class using specified list of <paramref name="matchers" />.
    /// </summary>
    /// <param name="matchers">A list of matchers for which at least one has to match.</param>
    public AnyMatcher(IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers)
    {
        Matchers = matchers ?? throw new ArgumentNullException(nameof(matchers));
        IsExclusive = false;
    }

    /// <summary>
    /// Gets the inner matchers.
    /// </summary>
    public IReadOnlyCollection<IAsyncHttpRequestMatcher> Matchers { get; }

    /// <inheritdoc />
    public Task<bool> IsMatchAsync(MockHttpRequestContext requestContext)
    {
        if (requestContext is null)
        {
            throw new ArgumentNullException(nameof(requestContext));
        }

        return Matchers.AnyAsync(requestContext);
    }

    /// <inheritdoc />
    public bool IsExclusive { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Matchers.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Any:");
        sb.Append('{');
        sb.AppendLine();
        foreach (IAsyncHttpRequestMatcher m in Matchers)
        {
            sb.Append('\t');
            sb.AppendLine(m.ToString());
        }

        sb.Append('}');
        return sb.ToString();
    }
}
