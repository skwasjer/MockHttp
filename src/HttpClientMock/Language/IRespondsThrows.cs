using System.ComponentModel;

namespace HttpClientMock.Language
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IRespondsThrows : IResponds, IThrows, IFluentInterface
	{
	}
}