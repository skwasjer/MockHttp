.NET library to mock HTTP responses for HttpClient and verify request expectations with an experience inspired by Moq.

## Documentation

Please see the [wiki for documentation](https://github.com/skwasjer/MockHttp/wiki).

## Usage example ###

```csharp
MockHttpHandler mockHttp = new MockHttpHandler();

// Configure setup(s).
mockHttp
    .When(matching => matching
        .Method("GET")
        .RequestUri("http://localhost/controller/*")
    )
    .Respond(HttpStatusCode.OK)
    .Verifiable();

var client = new HttpClient(mockHttp);

var response = await client.GetAsync("http://localhost/controller/action?test=1");

// Verify the expectations.
mockHttp.Verify();
```

### Contributions

Please check out the [contribution guidelines](https://github.com/skwasjer/MockHttp/blob/main/CONTRIBUTING.md).
