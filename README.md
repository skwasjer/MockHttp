# MockHttp

Collection of .NET libraries to mock HTTP responses for any HTTP client (`HttpClient`, `WebRequest`, `RestClient`, etc.)  and verify request expectations with an experience inspired by Moq.

---

[![Main workflow](https://github.com/skwasjer/MockHttp/actions/workflows/main.yml/badge.svg)](https://github.com/skwasjer/MockHttp/actions/workflows/main.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=skwasjer_MockHttp&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=skwasjer_MockHttp)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=skwasjer_MockHttp&metric=coverage)](https://sonarcloud.io/component_measures?id=skwasjer_MockHttp&metric=coverage)

| | | |
|---|---|---|
| `skwas.MockHttp` | [![NuGet](https://img.shields.io/nuget/v/skwas.MockHttp.svg)](https://www.nuget.org/packages/skwas.MockHttp/) [![NuGet](https://img.shields.io/nuget/dt/skwas.MockHttp.svg)](https://www.nuget.org/packages/skwas.MockHttp/) | [Documentation](https://github.com/skwasjer/MockHttp/wiki) |
| `skwas.MockHttp.Json` | [![NuGet](https://img.shields.io/nuget/v/skwas.MockHttp.Json.svg)](https://www.nuget.org/packages/skwas.MockHttp.Json/) [![NuGet](https://img.shields.io/nuget/dt/skwas.MockHttp.Json.svg)](https://www.nuget.org/packages/skwas.MockHttp.Json/) | [Documentation](https://github.com/skwasjer/MockHttp/wiki) |
| `skwas.MockHttp.Server` | [![NuGet](https://img.shields.io/nuget/v/skwas.MockHttp.Server.svg)](https://www.nuget.org/packages/skwas.MockHttp.Server/) [![NuGet](https://img.shields.io/nuget/dt/skwas.MockHttp.Server.svg)](https://www.nuget.org/packages/skwas.MockHttp.Server/) | [Documentation](https://github.com/skwasjer/MockHttp/wiki/Stubbing-an-API) |

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
    .Respond(with => with
        .StatusCode(200)
        .JsonBody(new { id = 123, firstName = "John", lastName = "Doe" })
    )
    .Verifiable();

var client = new HttpClient(mockHttp);

var response = await client.GetAsync("http://localhost/controller/action?test=1");

// Verify expectations.
mockHttp.Verify();
```

### Contributions

Please check out the [contribution guidelines](./CONTRIBUTING.md).
