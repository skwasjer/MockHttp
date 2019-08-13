using System.ComponentModel;

namespace MockHttp.Language.Flow
{
	/// <summary>
	/// Implements the fluent API.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ISequenceThrowsResult : IResponds<ISequenceResponseResult>, IThrows<ISequenceThrowsResult>, IThrowsResult, IVerifies, IFluentInterface
	{
	}
}