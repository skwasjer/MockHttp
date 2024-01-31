using System.Net;
using System.Text;
using MockHttp.FluentAssertions;
using Newtonsoft.Json;

namespace MockHttp.Json.Issues;

public class Issue98
{
    [Fact]
    public async Task When_setting_content_header_after_setting_jsonBody_it_should_return_expected_response()
    {
        using var mockHttp = new MockHttpHandler();
        mockHttp
            .When(m => m
                .Method(HttpMethod.Post)
                .RequestUri("api/login")
                .ContentType("application/json; charset=utf-8")
                .JsonBody(new { username = @"corp\user", password = "Super.Mari0.Bro$$" })
            )
            .Respond(r => r
                .StatusCode(HttpStatusCode.OK)
                .JsonBody(new { username = "user@corp" }, Encoding.UTF8)
                .Header("Set-Cookie",
                    "session=abcdefghi==; Expires=Tue, 01 Feb 2024 01:01:01 GMT; Secure; HttpOnly; Path=/")
            )
            .Verifiable();

        var client = new HttpClient(mockHttp)
        {
            BaseAddress = new Uri("http://localhost")
        };
        // Act
        HttpResponseMessage? response = await client.PostAsJsonAsync("api/login", new { username = @"corp\user", password = "Super.Mari0.Bro$$" });

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        response.Headers.Should()
            .ContainKey("Set-Cookie")
            .WhoseValue.Should().Contain("session=abcdefghi==; Expires=Tue, 01 Feb 2024 01:01:01 GMT; Secure; HttpOnly; Path=/");
        response.Should().HaveContentType("application/json; charset=utf-8");
        await response.Should().HaveContentAsync(JsonConvert.SerializeObject(new { username = "user@corp" }), Encoding.UTF8);
        mockHttp.Verify();
    }
}
