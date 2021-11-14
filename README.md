# MockHttp

Collection of .NET libraries to mock HTTP responses for any HTTP client (`HttpClient`, `WebRequest`, `RestClient`, etc.)  and verify request expectations with an experience inspired by Moq.

---

[![Build status](https://ci.appveyor.com/api/projects/status/n3t7ny3j7cryt92t/branch/main?svg=true)](https://ci.appveyor.com/project/skwasjer/mockhttp)
[![Tests](https://img.shields.io/appveyor/tests/skwasjer/mockhttp/main.svg)](https://ci.appveyor.com/project/skwasjer/mockhttp/build/tests)
[![codecov](https://codecov.io/gh/skwasjer/MockHttp/branch/main/graph/badge.svg)](https://codecov.io/gh/skwasjer/MockHttp)

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
    .RespondJson(HttpStatusCode.OK, new { id = 123, firstName = "John", lastName = "Doe" })
    .Verifiable();

var client = new HttpClient(mockHttp)

var response = await client.GetAsync("http://localhost/controller/action?test=1");

// Verify expectations.
mockHttp.Verify();
```

