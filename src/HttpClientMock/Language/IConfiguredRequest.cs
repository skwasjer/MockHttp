using System.ComponentModel;

namespace HttpClientMock.Language
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IConfiguredRequest : IResponds, IResponseResult, IThrows, IThrowsResult, ICallback, ICallbackResult, IFluentInterface
	{
	}
}