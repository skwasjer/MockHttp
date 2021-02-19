# Changelog

## v2.4.0

- Added `.Not` matcher (negating any matcher)
- Added request header name only matcher (see [#3](https://github.com/skwasjer/MockHttp/pull/3)).

## v2.3.0

- Added .NET 5.0 target framework support.
- Addressed static code analysis suggestions.

## v2.2.1

- Added [MockHttpServer](https://github.com/skwasjer/MockHttp/wiki/Stubbing-an-API).

## v2.2.0

- Added .NET Standard 2.1

## v2.1.1

- fix: `IndexOutOfRangeException` when matching by relative empty request uri

## v2.1.0

- Renamed a `Headers()` overload into `Header()` since it only matches 1 header.
- fix: throw ANE's from extensions to avoid potential NRE's.
- fix: SourceLink not working to due invalid (non-portable) symbols

## v2.0.8

- fix: matching on JSON arrays and simple values was throwing exception (ie.: `matching.JsonContent("match this")`).

## v2.0.4

- fix: Clearing invoked requests did not reset response sequence setups.

## v2.0

- Added support to chain responses, f.ex.: `.Throws(...).Respond(...).Respond(...)`.
- Added `TimesOut()` and `TimesOutAfter(millisecs)` response extensions.
- Renamed `VerifyNoOtherCalls()` to `VerifyNoOtherRequests()`, and fixed the verification not taking into account requests to `Verify(m => ....)`.
- Fix race condition when resetting mock, while an asynchronous request or expectation verification is in progress.
- Moved JSON extensions to `skwas.MockHttp.Json` package as to not require dependencies on [Json.NET](https://www.newtonsoft.com/) if one does not need JSON support.
- Added `VerifyAsync(Action<RequestMatching> matching, IsSent times, string because = null)` to avoid deadlocking when verifying `HttpContent`.
- Added `.FormData(...)` matcher.

## v1.3

- `.Url()` is replaced with `.RequestUri()`, but left in for compatibility (will be removed).
- added matcher to match request by HTTP message version.
- (breaking) replaced `IHttpRequestMatcher` with abstract class `HttpRequestMatcher`. This breaks the public API, but acceptable since no big audience ;)
- added more overloads for configuring responses.

## v1.2

- Fix: query string constraint for only a key `?key` (no = sign or value) is now accepted and will be matched as `null`, ie.: (`.QueryString("key", null)`).
- Fix: multiple partial match constraints can now be used.
- Fix: some matchers are mutually exclusive. An exception will be thrown if this is the case.
- Remove glob URI matcher for now, as this requires an extra dependency. Rethink this for future.
- Remove `TypeConverter` package dependency.

## v1.1

- Some refactoring of Fluent API.
- Add response extensions for HttpContent, Stream and others.

## v1.0

- Initial
