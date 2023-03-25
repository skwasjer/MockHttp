using MockHttp;
using MockHttp.Json;
using MyWebApp.Models;

namespace MyWebApp.Tests;

internal static class HttpMocks
{
    public static readonly IReadOnlyList<User> TestUsers = new List<User>
    {
        new(123, "John"),
        new(456, "Lisa"),
        new(789, "David")
    };

    public static MockHttpHandler CreateMyClientMock()
    {
        var mockHttpHandler = new MockHttpHandler();
        mockHttpHandler
            .When(matching => matching
                .Method(HttpMethod.Get)
                .RequestUri("/public/v2/users")
            )
            .Respond(with => with
                .JsonBody(TestUsers)
            )
            .Verifiable();

        return mockHttpHandler;
    }
}
