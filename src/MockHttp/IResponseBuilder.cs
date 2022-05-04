using System.ComponentModel;
using MockHttp.Responses;

namespace MockHttp;

/// <summary>
/// A builder to compose HTTP responses via a behavior pipeline.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IResponseBuilder : IFluentInterface
{
    /// <summary>
    /// Gets a list of behaviors to modify the HTTP response returned.
    /// </summary>
    IList<IResponseBehavior> Behaviors { get; }
}
