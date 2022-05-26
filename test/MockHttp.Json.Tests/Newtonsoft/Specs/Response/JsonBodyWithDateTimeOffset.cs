namespace MockHttp.Json.Newtonsoft.Specs.Response;

public class JsonBodyWithDateTimeOffset : Json.Specs.Response.JsonBodyWithDateTimeOffset
{
    protected override void Given(IResponseBuilder with)
    {
        with.JsonBody(new DateTimeOffset(2022, 5, 26, 10, 53, 34, 123, TimeSpan.FromHours(2)));
    }
}
