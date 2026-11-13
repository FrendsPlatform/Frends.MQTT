# Changelog

## [2.0.0] - 2026-11-11
### Changed
- [Breaking Change] Changed the Input class structure so that message is now separate object including message specific properties.
### Added
- Support for client certificate authentication.
- Option to receive the payload in binary format (base64 string).

## [1.1.0] - 2026-08-17
### Added
- Added an `Options` parameter to control error handling: use `ThrowErrorOnFailure` to throw on failure, or set it to `false` to receive the error details in the result instead.

## [1.0.0] - 2025-04-07
### Added
- Initial implementation