using System.ComponentModel;

namespace MockHttp.Language;

/// <summary>
/// Defines the <c>Verifiable</c> verb and overloads for verifying requests.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IVerifies : IFluentInterface
{
    /// <summary>
    /// Marks the expectation as verifiable, meaning that a call to <see cref="MockHttpHandler.Verify()" /> will check if this particular expectation was met.
    /// </summary>
    void Verifiable();

    /// <summary>
    /// Marks the expectation as verifiable, meaning that a call to <see cref="MockHttpHandler.Verify()" /> will check if this particular expectation was met, and specifies a reason for failures.
    /// </summary>
    void Verifiable(string because);
}
