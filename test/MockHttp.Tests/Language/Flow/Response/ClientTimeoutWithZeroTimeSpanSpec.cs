namespace MockHttp.Language.Flow.Response;

public class ClientTimeoutWithZeroTimeSpanSpec : ClientTimeoutSpec
{
    public ClientTimeoutWithZeroTimeSpanSpec()
        : base(TimeSpan.Zero)
    {
    }
}
