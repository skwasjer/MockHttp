using System.ComponentModel;
using MockHttp.Language.Flow;

namespace MockHttp.Language;

/// <summary>
/// Implements the fluent API.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IConfiguredRequest
    : IResponds<ISequenceResponseResult>, ISequenceResponseResult,
      IThrows<ISequenceThrowsResult>, ISequenceThrowsResult,
      ICallback<ISequenceResponseResult, ISequenceThrowsResult>, ICallbackResult<ISequenceResponseResult, ISequenceThrowsResult>,
      IFluentInterface
{
}
