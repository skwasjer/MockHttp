using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyWebApp.Models;
using Xunit;
using Xunit.Abstractions;

namespace MyWebApp.Tests;

/// <summary>
/// Tests the real world API without mocking or stubbing.
/// Provided for evidence in comparison to mocking or stubbing the HTTP client.
/// </summary>
public sealed class IntegrationTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public IntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task TestEndpoint()
    {
        await using var app = new WebApplicationFactory<Program>();

        // Act
        using HttpClient client = app.CreateClient();
        User[]? users = await client.GetFromJsonAsync<User[]>("/users");

        // Assert
        Assert.NotNull(users);
        Assert.NotEmpty(users);

        _testOutputHelper.WriteLine("User count: {0}", users.Length);
        foreach (User user in users)
        {
            _testOutputHelper.WriteLine("ID: {0}, Name: {1}", user.Id, user.Name);
        }
    }
}
