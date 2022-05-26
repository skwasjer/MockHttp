namespace MockHttp.Json.SystemTextJson.Specs.Response;

public class JsonBodyWithStringSpec : Json.Specs.Response.JsonBodyWithStringSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.JsonBody("some text");
    }
}
