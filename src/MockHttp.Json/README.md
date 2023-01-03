JSON extensions for [MockHttp](https://github.com/skwasjer/MockHttp).

## Documentation

Please see the [wiki for documentation](https://github.com/skwasjer/MockHttp/wiki).

## Usage example ###

```csharp
MockHttpHandler mockHttp = new MockHttpHandler();

// Configure setup(s).
mockHttp
    .When(matching => matching
        .JsonBody(new { id = 123 })
        .Method("GET")
        .RequestUri("http://localhost/controller/*")
    )
    .Respond(with => with
        .StatusCode(200)
        .JsonBody("<b>Hello world</b>")
        .ContentType("text/html")
    )
    .Verifiable();

var client = new HttpClient(mockHttp);

var response = await client.GetAsync("http://localhost/controller/action?test=1");

// Verify the expectations.
mockHttp.Verify();
```

## Other info

- [Changelog](https://github.com/skwasjer/MockHttp/blob/main/CHANGELOG.md)

### Contributions

Please check out the [contribution guidelines](https://github.com/skwasjer/MockHttp/blob/main/CONTRIBUTING.md).
