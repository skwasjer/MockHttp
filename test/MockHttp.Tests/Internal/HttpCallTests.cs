using MockHttp.Response;

namespace MockHttp;

public class HttpCallTests
{
    private readonly HttpCall _sut;

    public HttpCallTests()
    {
        _sut = new HttpCall();
        _sut.SetResponse(new ResponseFuncStrategy((_, __) => Task.FromResult(new HttpResponseMessage())));
    }

    [Fact]
    public async Task When_sending_response_should_reference_request()
    {
        var request = new HttpRequestMessage();

        // Act
        HttpResponseMessage response = await _sut.SendAsync(new MockHttpRequestContext(request), CancellationToken.None);

        // Assert
        response.RequestMessage.Should().Be(request);
    }
}
