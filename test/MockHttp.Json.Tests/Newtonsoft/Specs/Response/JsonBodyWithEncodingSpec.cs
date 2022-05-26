namespace MockHttp.Json.Newtonsoft.Specs.Response;

public class JsonBodyWithEncodingSpec : Json.Specs.Response.JsonBodyWithEncodingSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.JsonBody(new { firstName = "John", lastName = "Doe" }, Encoding);
    }
}
