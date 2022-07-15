#nullable enable
namespace MockHttp.Language.Flow.Response;

public class ServerTimeoutWithNullTimeSpanSpec : ServerTimeoutSpec
{
    public ServerTimeoutWithNullTimeSpanSpec()
        : base(null)
    {
    }
}
#nullable restore
