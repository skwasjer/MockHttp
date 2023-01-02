namespace MockHttp.Language.Flow.Response;

public class ServerTimeoutWithZeroTimeSpanSpec : ServerTimeoutSpec
{
    public ServerTimeoutWithZeroTimeSpanSpec()
        : base(TimeSpan.Zero)
    {
    }
}
