# Changelog

All notable changes to this project are documented here.

## [2.0.0-preview.1]

### Added

- Multi-targeting for .NET 8, 9, and 10.
- Structured `Microsoft.Extensions.Logging` output and custom sink registration.
- Privacy-conscious field selection and request filtering.
- Bounded, text-only request and response body capture with truncation indicators.
- Request and response body transforms for redaction.
- Bounded asynchronous JSON Lines file logging with daily rolling.
- Integration tests, sample API, public API compatibility checks, CI, and trusted publishing.

### Changed

- File logging is now part of `CD.RequestResponse.Middleware`.
- The public namespace is now `CD.RequestResponse`.
- The legacy file logger package is a compatibility dependency package.

### Removed

- Per-request external IP lookups.
- Automatic cookie and sensitive body collection.
- Synchronous request-path file writes.
