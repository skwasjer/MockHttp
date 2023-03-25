using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using MockHttp;
using MyWebApp.Models;
using Xunit;

namespace MyWebApp.Tests;

/// <summary>
/// Tests with a mocked client.
/// The benefit is you cover all your own application code, including delegating handlers. The main benefit over the smoke tests with a server stub is speed of the test. No time is wasted setting up a stub server.
/// </summary>
public sealed class MockedTests
{
    [Fact]
    public async Task TestEndpoint()
    {
        // Configure HTTP mock.
        using MockHttpHandler mockHttpHandler = HttpMocks.CreateMyClientMock();

        // Update HttpClientFactory configuration for the named or typed client.
        await using WebApplicationFactory<Program> app = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Here we replace the primary handler configured for the API client.
                builder.ConfigureServices(services => services
                    .Configure<HttpClientFactoryOptions>("my-client",
                        options =>
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            options.HttpMessageHandlerBuilderActions.Add(handler => handler.PrimaryHandler = mockHttpHandler);
                        })
                );
            });

        // Act
        using HttpClient client = app.CreateClient();
        User[]? users = await client.GetFromJsonAsync<User[]>("/users");

        // Assert
        Assert.NotNull(users);
        Assert.Equivalent(HttpMocks.TestUsers, users); // We should have matching users :D

        mockHttpHandler.Verify();
    }
}
