#nullable enable
namespace MockHttp.Language.Flow.Response;

public class ClientTimeoutWithNullTimeSpanSpec : ClientTimeoutSpec
{
    public ClientTimeoutWithNullTimeSpanSpec()
        : base(null)
    {
    }
}
#nullable restore
