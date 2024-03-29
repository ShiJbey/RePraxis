# Changelog

All notable changes to this project will be documented in this file.

The format is based on <https://common-changelog.org/>, and this project adheres mostly to Semantic Versioning. However, all releases before 1.0.0 have breaking changes between minor-version updates.

## [1.3.0] - 2024-02-06

### Changed

- **breaking change**: `QueryResult.Bindings` is now of type `Dictionary<string, object>` mapping variables to their parsed values
- **breaking change**: `IQueryExpression.Evaluate()` now accepts and returns a `QueryState` rather than a `QueryResult`.
- Refactored internal project structure.

### Added

- Added `Clear()` Method to `RePraxisDatabase` as an alias to `RePraxisDatabase.Root.ClearChildren()`.
- Added `QueryResult.LimitToVars()` method to simplify limiting query results to a set of variables.
- Added `QueryState` class to track intermediate query results

## [1.2.0] - 2024-01-08

### Added

- Users can supply an array of bindings to queries
- Add `QueryResult.ToPrettyString()` to simplify viewing query results
- Update README to better explain query statements

### Fixed

- Not-statement inconsistent results from unclear semantics
- Empty initial bindings causing query to fail

## [1.1.0] - 2024-01-05

### Added

- Add option to supply initial bindings to query
- Add bundler script to help with making releases

### Fixed

- Fix typo in unit test

## [1.0.0] - 2023-12-31

### Added

- Add `RePraxisDatabase` class and `Insert`, `Delete`, and `Assert` methods.
- Add nodes to support variables, symbols, integers and floats
- Add query interface with support for assertion, negation, and relational operations

_Initial release._

[1.0.0]: https://github.com/ShiJbey/RePraxis/releases/tag/v1.0.0
[1.1.0]: https://github.com/ShiJbey/RePraxis/releases/tag/v1.1.0
[1.2.0]: https://github.com/ShiJbey/RePraxis/releases/tag/v1.2.0
[1.3.0]: https://github.com/ShiJbey/RePraxis/releases/tag/v1.3.0
