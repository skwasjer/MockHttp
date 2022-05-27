﻿namespace MockHttp.Json.Newtonsoft.Specs.Response;

public class JsonBodySpec : Json.Specs.Response.JsonBodySpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.JsonBody(new { firstName = "John", lastName = "Doe" });
    }
}
