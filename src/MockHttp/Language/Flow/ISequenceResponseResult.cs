using System.ComponentModel;

namespace MockHttp.Language.Flow;

/// <summary>
/// Implements the fluent API.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ISequenceResponseResult : IResponds<ISequenceResponseResult>, IThrows<ISequenceThrowsResult>, IResponseResult, IVerifies, IFluentInterface
{
}
