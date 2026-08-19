# Changelog

## [2.0.0] - 2026-08-17
### Added
- [Breaking Change] Change Error type in response from string to custom error object
- Added an `Options` parameter to the Send task, allowing you to control whether errors are thrown or returned as a result (`ThrowErrorOnFailure`) and to provide a custom error message (`ErrorMessageOnFailure`).

## [1.0.0] - 2025-04-07
### Added
- Initial implementation
