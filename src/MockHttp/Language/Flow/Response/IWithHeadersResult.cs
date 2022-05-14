using System.ComponentModel;
using MockHttp.Language.Response;

namespace MockHttp.Language.Flow.Response;

/// <summary>
/// Implements the fluent API.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IWithHeadersResult
    : IWithHeaders,
      IFluentInterface
{
}
