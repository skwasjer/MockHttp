# Changelog

## v4.3.1

* fix: setting any content header (specifically) after setting JSON body resets `HttpContent` to empty by @skwasjer in https://github.com/skwasjer/MockHttp/pull/99

**Full Changelog**: [v4.3.0...v4.3.1](https://github.com/skwasjer/MockHttp/compare/v4.3.0...v4.3.1)

## v4.3.0

* feat(net8): add .NET 8 target framework by @skwasjer in https://github.com/skwasjer/MockHttp/pull/86
* feat(server): implement `IAsyncDisposable` on `HttpMockServer` by @skwasjer in https://github.com/skwasjer/MockHttp/pull/89
* fix(server): `MockHttpServer.StopAsync` disposes the host before it has a chance to actually stop by @skwasjer in https://github.com/skwasjer/MockHttp/pull/88
* fix(xml): do not modify `XmlSettings` supplied to `XmlBody` response builder extension by @skwasjer in https://github.com/skwasjer/MockHttp/pull/90
* feat: added ctor parameter to `RateLimitedStream` to keep the underlying stream open on disposal by @skwasjer in https://github.com/skwasjer/MockHttp/pull/95
* feat: added `TransferRate` extension to simulate a slow network transport by @skwasjer in https://github.com/skwasjer/MockHttp/pull/96

### Chores
* fix: address some static code analysis issues, exposed by the latest analyzer (in .NET 8 SDK) by @skwasjer in https://github.com/skwasjer/MockHttp/pull/87
* fix: force GC collect (in unit test) and finalize on appdomain unload by @skwasjer in https://github.com/skwasjer/MockHttp/pull/91
* chore(deps): bump xunit from 2.6.1 to 2.6.2 by @dependabot in https://github.com/skwasjer/MockHttp/pull/92

**Full Changelog**: [v4.2.0...v4.3.0](https://github.com/skwasjer/MockHttp/compare/v4.2.0...v4.3.0)

## v4.2.0

* Address several static code analysis warnings by @skwasjer in https://github.com/skwasjer/MockHttp/pull/84
  * fix(CA2007): `ConfigureAwait(false)` a task
  * style(CA1819): properties should not return arrays (refactored private/internal use only)
  * style(CA1307,CA1309): Use ordinal string comparison overload
  * style(CA8602): suppress dereference possible null in test
  * style(CA2000): suppress false positive because it is registered for dispose
  * style(CA2201): suppress
  * style(CA1859): change interface type to concrete type for perf.
  * style(CA1054,CA1056): change type of parameter from string to Uri.
    * The `MockHttpServer.HostUrl` is deprecated. Use `MockHttpServer.HostUri` instead.
    * The `MockHttpServer` ctors accepting `string` URL's are deprecated. Use the overloads accepting `System.Uri`.
  * style(CA1861): prefer 'static readonly' fields over constant array args.
  * style(CA1860): prefer comparing count to 0 rather using Any()
  * style(CA5394): do not use insecure randomness, suppressed false positive

### Chores
* ci: refactored CI, fixing SonarCloud/CodeCov integration, added CodeQL by @skwasjer in https://github.com/skwasjer/MockHttp/pull/71
* chore(deps): bump FluentAssertions from 6.11.0 to 6.12.0 by @dependabot in https://github.com/skwasjer/MockHttp/pull/70
* chore(deps): bump NetTestSdkVersion from 17.7.0 to 17.7.1 by @dependabot in https://github.com/skwasjer/MockHttp/pull/69
* chore(deps): bump NetTestSdkVersion from 17.7.1 to 17.7.2 by @dependabot in https://github.com/skwasjer/MockHttp/pull/72
* chore(deps): bump Microsoft.AspNet.WebApi.Client from 5.2.9 to 6.0.0 by @dependabot in https://github.com/skwasjer/MockHttp/pull/74
* chore(deps): bump NetTestSdkVersion from 17.7.2 to 17.8.0 by @dependabot in https://github.com/skwasjer/MockHttp/pull/77
* chore(deps): bump xunit from 2.4.2 to 2.6.1 by @dependabot in https://github.com/skwasjer/MockHttp/pull/76
* chore(deps): bump Serilog from 3.0.1 to 3.1.1 by @dependabot in https://github.com/skwasjer/MockHttp/pull/79
* test(deps): replace Moq with NSubstitute by @skwasjer in https://github.com/skwasjer/MockHttp/pull/85
* ci: switch fully to GitHub actions instead of AppVeyor by @skwasjer in https://github.com/skwasjer/MockHttp/pull/83

**Full Changelog**: [v4.1.1...v4.2.0](https://github.com/skwasjer/MockHttp/compare/v4.1.1...v4.2.0)

## 4.1.1

- fix: data escaped + (plus) was not parsed correctly, caused by [#68](https://github.com/skwasjer/MockHttp/pull/68). My apologies... :)

## v4.1.0

- fix: data escaped space (+) was not parsing into space char, causing form data matcher to fail to match. by @skwasjer in [#68](https://github.com/skwasjer/MockHttp/pull/68)
- chore(deps): replaced Microsoft.AspNet.WebApi.Client with explicit dependencies on Newtonsoft.Json and System.Net.Http to address a security concern regarding transitive dependency on NewtonSoft < 13.x. See  [GHSA-5crp-9r3c-p9vr](https://github.com/advisories/GHSA-5crp-9r3c-p9vr).

### Chores

- fix: .NET Standard 2.0 was not covered.  by @skwasjer in [#52](https://github.com/skwasjer/MockHttp/pull/52)
- chore(deps): bump NetTestSdkVersion from 17.5.0 to 17.6.0 by @dependabot in [#55](https://github.com/skwasjer/MockHttp/pull/55)
- chore(deps): bump FluentAssertions from 6.10.0 to 6.11.0 by @dependabot in [#53](https://github.com/skwasjer/MockHttp/pull/53)
- chore(deps): bump NetTestSdkVersion from 17.6.0 to 17.6.2 by @dependabot in [#57](https://github.com/skwasjer/MockHttp/pull/57)
- ci: .NET Framework test builds fail with .NET 8 SDK preview by @skwasjer in [#65](https://github.com/skwasjer/MockHttp/pull/65)
- ci(deps): bump SonarScanner dependencies by @skwasjer in [#66](https://github.com/skwasjer/MockHttp/pull/66)
- chore(deps): bump NetTestSdkVersion from 17.6.2 to 17.7.0 by @dependabot in [#64](https://github.com/skwasjer/MockHttp/pull/64)

## v4.0.1

- Add timeout exception for .NET 5 or greater targets. by @benjaminsampica in [#50](https://github.com/skwasjer/MockHttp/pull/50)
- Improve `RateLimitedStream` timing/bit rate calculation accuracy.
- Fix guard/exception for the minimum allowed bit rate for `RateLimitedStream`.
- Fix underlying stream not being disposed when calling `RateLimitedStream.Dispose()`

## v4.0.0
- Add new Fluent response API by @skwasjer in [#10](https://github.com/skwasjer/MockHttp/pull/10) See also [#9](https://github.com/skwasjer/MockHttp/issues/9)
  This is a complete replacement of the old response builder API's, and thus likely will require you to refactor your tests (I'm sorry but such is the price of improvement sometimes :( ). See wiki for new API docs/examples. The request matching API's have largely stayed the same, but some extension methods were renamed for consistency.
- Added .NET 7 target framework
- Add stream that rate limits response streams. by @skwasjer in [#17](https://github.com/skwasjer/MockHttp/pull/17)
- Additional guards (Argument(Null)Exception) for certain extensions.
- Fix static code analysis warnings.
- Bump Microsoft.AspNet.WebApi.Client from 5.2.7 to 5.2.9 by @dependabot in [#23](https://github.com/skwasjer/MockHttp/pull/23)
- Enable nullable by @skwasjer in [#27](https://github.com/skwasjer/MockHttp/pull/27)
- Remove obsolete/deprecated code (+semver:major) by @skwasjer in [#16](https://github.com/skwasjer/MockHttp/pull/16)
- Removed obsolete `ObjectResponseStrategy` and its extensions.
- Removed obsolete `MockHttpHandler.VerifyNoOtherCalls()`. Use the replacement `VerifyNoOtherRequests()`
- Removed obsolete `UrlMatcher` and its extensions.

## v3.1.0-rc0003

- Added `.XmlBody()` response builder extension for LINQ to XML.

## v3.1.0-rc0002

- fix: allow zero timespan in `ServerTimeout()` and `ClientTimeout()`.
- fix: do not allow `RateLimitedStream` if the underlying stream is not seekable (except for via stream factory).

## v3.1.0-rc0001

- Added new fluent response API which provides more flexibility and control over the response. This new API replaces the current `.Respond()` API, and as such most of the old methods/overloads are now deprecated and will be removed in v4.
- Added `RateLimitedStream` helper to simulate network transfer rates.
- Renamed `Content`, `PartialContent`, `JsonContent` request matchers to `Body`, `PartialBody`, `JsonBody` respectively. The old methods are obsolete and will be removed in v4.

## v3.0.1

- fix: stop evaluating next matchers as soon as a failed match is encountered.

## v3.0.0

### MockHttp

- Added .NET 6.0 and .NET Framework 4.8/4.7.2/4.6.2 target framework support.
- Removed .NET Standard < 2 and .NET Framework 4.5 support.

### MockHttp.Json

- Changed to System.Text.Json as default serializer, JSON.NET can be configured as default if desired (`mockHttpHandler.UseNewtonsoftJson()`).
- Added .NET 6.0 and .NET Framework 4.8/4.7.2/4.6.2 target framework support.
- Removed .NET Standard < 2 and .NET Framework 4.5 support.

### MockHttp.Server

- Added .NET 6 and .NET Core 3.1 target framework support.
- Removed .NET Standard 2.x target frameworks support.

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
