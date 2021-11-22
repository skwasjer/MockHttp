JSON extensions for [MockHttp](https://github.com/skwasjer/MockHttp).

## Documentation

Please see the [wiki for documentation](https://github.com/skwasjer/MockHttp/wiki).

## Usage example ###

```csharp
MockHttpHandler mockHttp = new MockHttpHandler();

// Configure setup(s).
mockHttp
    .When(matching => matching
        .JsonContent(new { id = 123 })
        .Method("GET")
        .RequestUri("http://localhost/controller/*")
    )
    .RespondJson(HttpStatusCode.OK, new { id = 123, firstName = "John", lastName = "Doe" })
    .Verifiable();

var client = new HttpClient(mockHttp);

var response = await client.GetAsync("http://localhost/controller/action?test=1");

// Verify the expectations.
mockHttp.Verify();
```

### Contributions

Please check out the [contribution guidelines](https://github.com/skwasjer/MockHttp/blob/main/CONTRIBUTING.md).
