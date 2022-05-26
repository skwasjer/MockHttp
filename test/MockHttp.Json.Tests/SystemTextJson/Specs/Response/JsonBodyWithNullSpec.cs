namespace MockHttp.Json.SystemTextJson.Specs.Response;

public class JsonBodyWithNullSpec : Json.Specs.Response.JsonBodyWithNullSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.JsonBody((object?)null);
    }
}
