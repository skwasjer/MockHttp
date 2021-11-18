using System.ComponentModel;

namespace MockHttp.Language.Flow;

[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class FallbackRequestSetupPhrase : SetupPhrase<IResponseResult, IThrowsResult>, IRespondsThrows, IFluentInterface
{
    public FallbackRequestSetupPhrase(HttpCall setup)
        : base(setup)
    {
    }
}
