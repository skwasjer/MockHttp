using System.ComponentModel;
using MockHttp.Language.Flow;

namespace MockHttp.Language;

/// <summary>
/// Defines the <c>Responds</c> and <c>Throws</c> verb.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IRespondsThrows : IResponds<IResponseResult>, IThrows<IThrowsResult>, IFluentInterface
{
}