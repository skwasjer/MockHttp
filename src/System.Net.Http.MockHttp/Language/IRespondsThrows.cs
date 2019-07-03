using System.ComponentModel;

namespace System.Net.Http.MockHttp.Language
{
	/// <summary>
	/// Defines the <c>Responds</c> and <c>Throws</c> verb.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IRespondsThrows : IResponds, IThrows, IFluentInterface
	{
	}
}