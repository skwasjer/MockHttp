using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MockHttp;
using MyWebApp.Configuration;
using MyWebApp.Models;
using Xunit;

namespace MyWebApp.Tests;

/// <summary>
/// Tests with a server stub.
/// The benefit is you cover all your own application code, including delegating handlers. The main benefit over the mocked tests is that as far as your app is concerned, it is talking to a real API. This includes low-level socket operations, network streaming, content negotiation, redirects, etc.
/// The downside is it takes quite a bit more 'run' time to spin up the server, causing overall test runs to take longer. This could be sped up by moving the server/bootstrapping to a xunit-fixture (one-time/global setup in NUNit) so it can be reused and does not have to be restarted each test, but will still be slower due to the latency imposed by running another .NET API in the background (besides your own). One now also has to worry somewhat about threading when setting up your mocks.
/// </summary>
public sealed class SmokeTests // aka. with a server stub for the HttpClient.
{
    [Fact]
    public async Task TestEndpoint()
    {
        // Configure HTTP mock.
        using MockHttpHandler mockHttpHandler = HttpMocks.CreateMyClientMock();
        // We create a server with automatic port binding (0) on localhost, but you may also define a static port. Note that the port must be free!
        using var mockHttpServer = new MockHttpServer(mockHttpHandler, "http://127.0.0.1:0");

        await mockHttpServer.StartAsync();

        try
        {
            await using WebApplicationFactory<Program> app = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // Here we override the base URL configured for the API client with the one from the server stub.
                    builder.ConfigureServices(services => services
                        // ReSharper disable once AccessToDisposedClosure
                        .Configure<MyClientOptions>(opts => opts.BaseUrl = new Uri(mockHttpServer.HostUrl))
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
        finally
        {
            await mockHttpServer.StopAsync();
        }
    }
}
