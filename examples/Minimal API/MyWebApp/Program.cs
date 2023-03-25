using Microsoft.Extensions.Options;
using MyWebApp.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .Configure<MyClientOptions>(builder.Configuration.GetSection("MyClient").Bind)
    .AddHttpClient("my-client", // using a named client, but in principal it is the same for a typed client.
        (services, client)
            => client.BaseAddress = services.GetRequiredService<IOptions<MyClientOptions>>().Value.BaseUrl
    );

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapGet("users",
        async (IHttpClientFactory httpClientFactory) =>
        {
            using HttpClient client = httpClientFactory.CreateClient("my-client");
            return await client.GetStringAsync("/public/v2/users");
        })
    .Produces(StatusCodes.Status200OK, typeof(string), "application/json");

app.Run();

// For testing minimal API's with test server.
public partial class Program { }
