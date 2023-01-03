.NET library to mock/stub HTTP responses and verify request expectations with an experience inspired by Moq.

## Documentation

Please see the [wiki for documentation](https://github.com/skwasjer/MockHttp/wiki).

## Usage example ###

```csharp
// Create the mock handler.
MockHttpHandler mockHttp = new MockHttpHandler();

// Configure setup(s).
mockHttp
    .When(matching => matching.Method("GET"))
    .Respond(with => with
        .StatusCode(200)
        .JsonBody("<b>Hello world</b>")
        .ContentType("text/html")
    )
    .Verifiable();

// Mount the mock handler in a server.
// Specify port 0 to bind to a free port, or otherwise provide an unused/free port.
MockHttpServer server = new MockHttpServer(mockHttp, "http://127.0.0.1:0");
await server.StartAsync();

// Configure the subject under test to use the same host and port.
// For sake of example, we're using WebRequest here.

// Act
var request = WebRequest.Create($"{server.HostUrl}/controller/action");
request.Method = "GET";

// Assert
HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
response.StatusCode.Should().Be(HttpStatusCode.OK);

// Verify the request expectation as configured on the mock handler.
mockHttp.Verify();

// Clean up
await server.StopAsync();
server.Dispose()
```

## Other info

- [Changelog](https://github.com/skwasjer/MockHttp/blob/main/CHANGELOG.md)

### Contributions

Please check out the [contribution guidelines](https://github.com/skwasjer/MockHttp/blob/main/CONTRIBUTING.md).
