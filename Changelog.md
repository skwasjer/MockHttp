# Changelog

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