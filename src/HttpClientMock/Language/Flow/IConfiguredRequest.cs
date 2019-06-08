using System.ComponentModel;

namespace HttpClientMock.Language.Flow
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IConfiguredRequest : IResponds, IResponseResult, IThrows, IThrowsResult, IRespondsThrows, ICallback, ICallbackResult, IFluentInterface
	{
	}
}