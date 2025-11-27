using System.ComponentModel;

namespace MockHttp.Language;

/// <summary>
/// Defines a builder interface for constructing a collection of match rules.
/// </summary>
/// <typeparam name="T">The type of the match rule.</typeparam>
/// <typeparam name="TSelf">The type of the implementing builder, enabling fluent chaining.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IMatchBuilder<T, out TSelf>
    where TSelf : IMatchBuilder<T, TSelf>
{
    /// <summary>
    /// Adds a new match rule.
    /// </summary>
    /// <param name="matcher">The matcher to add.</param>
    /// <returns>The builder to continue chaining.</returns>
    public TSelf Add(T matcher);

    /// <summary>
    /// Builds and returns a read-only collection of match rules that have been added to the builder.
    /// </summary>
    /// <returns>A read-only collection of match rules.</returns>
    public IReadOnlyCollection<T> Build();
}
