# MockHttp

Mock HTTP responses for `HttpClient` and verify request expectations, with an experience inspired by Moq.

## Installation

Install MockHttp via the Nuget package manager or `dotnet` cli.

```powershell
dotnet add package skwas.MockHttp
```

For JSON or `MediaTypeFormatter` integration

```powershell
dotnet add package skwas.MockHttp.Json
```

---

[![Build status](https://ci.appveyor.com/api/projects/status/n3t7ny3j7cryt92t/branch/master?svg=true)](https://ci.appveyor.com/project/skwasjer/mockhttp)
[![Tests](https://img.shields.io/appveyor/tests/skwasjer/mockhttp/master.svg)](https://ci.appveyor.com/project/skwasjer/mockhttp/build/tests)
[![codecov](https://codecov.io/gh/skwasjer/MockHttp/branch/master/graph/badge.svg)](https://codecov.io/gh/skwasjer/MockHttp)

| | | |
|---|---|---|
| `skwas.MockHttp` | [![NuGet](https://img.shields.io/nuget/v/skwas.MockHttp.svg)](https://www.nuget.org/packages/skwas.MockHttp/) [![NuGet](https://img.shields.io/nuget/dt/skwas.MockHttp.svg)](https://www.nuget.org/packages/skwas.MockHttp/) | |

## Usage ###

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

## Matching a request

MockHttp provides a fluent API to make setting up request expectations and verifications easy.

| Method | Description |
| -- | -- |
| <pre>.Method("POST")<br/>.Method(HttpMethod.Put)</pre> | Matches the request method. | 
| <pre>.RequestUri("http://localhost/exact/match?query=string")<br/>.RequestUri("*/match\*")</pre> | Matches an url. Accepts wildcard `*`. | 
| <pre>.QueryString("key", "value")<br/>.QueryString("?key=value1&other=value&key=value2")<br/>.QueryString(new Dictionary<string, string><br/>{<br/>    { "key", "value" }<br/>}<br/>.QueryString(new NameValueCollection<br/>{<br/>    { "key", "value" }<br/>}</pre> | Matches a query string by one or more key/value pairs. Note: the overload that accepts a full query string must be URI data escaped. |
| <pre>.WithoutQueryString()</pre> | Matches a request without query string. |
| <pre>.Content("text content")<br/>.Content("text content", Encoding.UTF8)<br/>.Content(stream)<br/>.Content(byteArray)</pre> | Matches the request content body. |
| <pre>.WithoutContent()</pre> | Matches a request without content. |
| <pre>.PartialContent("text content")<br/>.PartialContent("text content", Encoding.UTF8)<br/>.PartialContent(byteArray)</pre> | Matches the content body partially. |
| <pre>.ContentType("application/json; charset=utf-8")<br/>.ContentType("text/plain", Encoding.ASCII)</pre> | Matches the request content type. | 
| <pre>.Header("Authorization", "Bearer 123")<br/>.Header("Content-Length", 123)<br/>.Header("Last-Modified", DateTime.UtcNow)<br/>.Headers("Accept: text/html")<br/>.Headers(new Dictionary<string, string><br/>{<br/>    { "Authorization", "Bearer 123" }<br/>})</pre> | Matches request headers by one or more key/value pairs. | 
| <pre>.FormData("key", "value")<br/>.FormData("key=value1&other=value&key=value2")<br/>.FormData(new Dictionary<string, string><br/>{<br/>    { "key", "value" }<br/>}<br/>.FormData(new NameValueCollection<br/>{<br/>    { "key", "value" }<br/>}<br/></pre> | Matches form url encoded or multipart encoded form data by one or more key/value pairs. Note: the overload that accepts a string must be URI data escaped. |
| <pre>.Any(any => any<br/>    .Method("GET")<br/>    .Method("POST")<br/>)</pre> | Matches when at least one of the inner configured conditions is true. | 
| <pre>.Where(request => true\|false)</pre> | Matches the request using a custom predicate/expression. | 
| <pre>.Version("2.0")<br/>.Version(new Version(1, 1))</pre> | Matches the request by HTTP message version. | 

## Configuring a response

There are several overloads to configure a response. Here are just a few:

```csharp
mockHttp
    .When(...)
    .Respond(HttpStatusCode.OK, "text data", "text/plain");

mockHttp
    .When(...)
    .Respond(request => new HttpResponseMessage(HttpStatusCode.BadRequest));

mockHttp
    .When(...)
    .Respond(async request => await ...);

mockHttp
    .When(...)    
    .RespondJson(new Person { FullName = "John Doe" })  // Requires 'skwas.MockHttp.Json' package
```

## Throwing an exception

To throw an exception in response to a request:

```csharp
mockHttp
    .When(...)
    .Throws<InvalidOperationException>();
```

## Configuring response with custom strategy

For more complex response configurations and/or reusability implement `IResponseStrategy`.

```csharp
public class MyResponseStrategy : IResponseStrategy
{
    public Task<HttpResponseMessage> ProduceResponseAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
    {
        // Custom response logic.        
    }
}

mockHttp
    .When(...)
    .RespondUsing(new MyResponseStrategy());
```

An added benefit is it helps to keep the unit tests themselves clean.


## Setting up a response sequence

Multiple responses can be configured to form a sequence. This is useful when a request is expected to happen multiple times, but with different responses.

The `Respond`, `RespondUsing`, `RespondJson`, `Throws`, `TimesOut`, `TimesOutAfter` response configuration extensions can be chained to form a sequence.

```csharp
mockHttp
    .When(...)
    .Respond(HttpStatusCode.BadGateway)
    .TimesOutAfter(500)
    .Respond(HttpStatusCode.Ok)
    .Throws<HttpRequestException>()
    .Respond(HttpStatusCode.Ok)
    .RespondUsing(new MyResponseStrategy())
    .Respond(HttpStatusCode.Ok)
```

> The last configured response will be repeated if more requests are executed.

## Verifying requests

Similar to Moq, each setup can be configured as `Verifiable()`, turning it into an expectation that can be verified later on using `Verify()`.

```csharp
mockHttp
    .When(...)
    .Respond(...)
    .Verifiable();

mockHttp.Verify();
```

When the expectation is not met a `HttpMockException` is thrown when calling `Verify()` which provides details on which expectations were not met.

| Verify methods | Description |
| -- | -- |
| `Verify()` | Verifies that all verifiable expected requests have been sent. |
| `VerifyAll()` | Verifies all expected requests regardless of whether they have been flagged as verifiable. |
| `VerifyNoOtherRequests()` | Verifies that there were no requests sent other than those already verified. |

> When a response sequence is configured, the setup will only match if at least that amount of requests have been sent.

### Verifying without verifiable expected requests

You also have the option, similar to Moq, to verify without having configured a verifiable expected request. For example, to verify a specific url has been called twice:

```csharp
await mockHttp.VerifyAsync(matching => matching.RequestUri("http://localhost/controller/*"), IsSent.Exactly(2));
```

## Callback

When a callback is configured, it will be called before the response is produced and returned. This can be useful to track custom state or manipulate the response further.

```csharp
mockHttp
    .When(...)
    .Callback(request => { /* Do something */ })
    .Respond(...)
```

## Default response

Configure a default response using the `Fallback` property, which will be used to return a response when the request does not match any setup.

```csharp
mockHttp.Fallback.Respond(HttpStatusCode.BadRequest);
```

By default, `HttpStatusCode.NotFound` is returned.

## More info

### Supported .NET targets
- .NET Standard 2.0
- .NET Standard 1.1
- .NET Framework 4.5

### Build requirements
- Visual Studio 2017
- .NET Core 2.2/2.1 SDK
- .NET 4.62

### Contributions
PR's are welcome. Please rebase before submitting, provide test coverage, and ensure the AppVeyor build passes. I will not consider PR's otherwise.

### Contributors
- skwas (author/maintainer)

### Inspired by

- [Moq](https://github.com/moq/moq)
- [RichardSzalay.MockHttp](https://github.com/richardszalay/mockhttp)
