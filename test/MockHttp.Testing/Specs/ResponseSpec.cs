#nullable enable
using System.Net;
using FluentAssertions;
using MockHttp.Language;
using Xunit;

namespace MockHttp.Specs;

public abstract class ResponseSpec : IAsyncLifetime
{
    private readonly MockHttpHandler _mockHttp;
    private readonly HttpClient? _httpClient;
    private readonly IConfiguredRequest _configuredRequest;

    protected ResponseSpec()
    {
        _mockHttp = new MockHttpHandler();
        _httpClient = new HttpClient(_mockHttp)
        {
            BaseAddress = new Uri("http://0.0.0.0")
        };
        _configuredRequest = _mockHttp.When(_ => { });
    }

    [Fact]
    public async Task Act()
    {
        HttpResponseMessage response = await When(_httpClient!);

        await Should(response);
    }

    protected abstract void Given(IResponseBuilder with);

    protected virtual Task<HttpResponseMessage> When(HttpClient httpClient)
    {
        _configuredRequest.Respond(Given);
        return Send(httpClient);
    }

    protected virtual Task<HttpResponseMessage> Send(HttpClient httpClient)
    {
        return httpClient.GetAsync("");
    }

    protected virtual Task Should(HttpResponseMessage response)
    {
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        return Task.CompletedTask;
    }

    Task IAsyncLifetime.InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        _httpClient?.Dispose();
        _mockHttp.Dispose();
        return Task.CompletedTask;
    }
}
#nullable restore
