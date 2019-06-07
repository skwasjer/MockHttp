using System.ComponentModel;

namespace HttpClientMock.Language
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IVerifies : IFluentInterface
	{
		void Verifiable();

		void Verifiable(string because);
	}
}