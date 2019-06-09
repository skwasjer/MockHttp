using System.ComponentModel;

namespace HttpClientMock.Language.Flow
{
	/// <summary>
	/// Implements the fluent API.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ICallbackResult : IResponds, IThrows, IFluentInterface
	{
	}
}