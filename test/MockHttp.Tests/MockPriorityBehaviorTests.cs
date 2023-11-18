using System.Net;

namespace MockHttp;

public class MockPriorityBehaviorTests
{
    [Fact]
    public async Task Given_request_is_setup_twice_when_sending_request_last_setup_wins()
    {
        using var httpMock = new MockHttpHandler();
        using var httpClient = new HttpClient(httpMock) { BaseAddress = new Uri("http://0.0.0.0") };
        httpMock
            .When(matching => matching.Method("POST"))
            .Respond(with => with.StatusCode(HttpStatusCode.OK));
        httpMock
            .When(matching => matching.Method("POST"))
            .Respond(with => with.StatusCode(HttpStatusCode.Accepted));
        httpMock
            .When(matching => matching.Method("PUT"))
            .Respond(with => with.StatusCode(HttpStatusCode.BadRequest));

        // Act
        HttpResponseMessage response1 = await httpClient.PostAsync("", new StringContent("data 1"));
        HttpResponseMessage response2 = await httpClient.PostAsync("", new StringContent("data 2"));
        HttpResponseMessage response3 = await httpClient.PutAsync("", new StringContent("data 3"));

        // Assert
        response1.Should().HaveStatusCode(HttpStatusCode.Accepted, "the second setup wins on first request");
        response2.Should().HaveStatusCode(HttpStatusCode.Accepted, "the second setup wins on second request");
        response3.Should().HaveStatusCode(HttpStatusCode.BadRequest, "the request was sent with different HTTP method matching third setup");
    }
}
