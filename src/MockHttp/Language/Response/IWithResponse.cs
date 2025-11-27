using System.ComponentModel;
using MockHttp.Response;

namespace MockHttp.Language.Response;

/// <summary>
/// Describes a way to build a response via a behavior pipeline.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IWithResponse
    : IFluentInterface
{
    /// <summary>
    /// Gets a list of behaviors to modify the HTTP response returned.
    /// </summary>
    IList<IResponseBehavior> Behaviors { get; }
}
