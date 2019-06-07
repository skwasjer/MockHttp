using System.ComponentModel;

namespace HttpClientMock.Language
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ICallbackResult : IResponds, IThrows, IFluentInterface
	{
	}
}