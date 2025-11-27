using System.ComponentModel;
using MockHttp.Matchers;

namespace MockHttp.Language;

/// <summary>
/// Defines an interface for configuring and building HTTP request matchers.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IRequestMatching : IMatchBuilder<IAsyncHttpRequestMatcher, IRequestMatching>
{
    /// <summary>
    /// Gets a builder that inverts the logic of the match rules.
    /// </summary>
    /// <remarks>
    /// When using the <see cref="Not" /> property, the match rules added to the builder will be negated.
    /// This is useful for specifying conditions that should not match the request.
    /// </remarks>
    /// <value>
    /// A builder that inverts the match rules.
    /// </value>
#pragma warning disable CA1716
    public IRequestMatching Not { get; }
#pragma warning restore CA1716
}
