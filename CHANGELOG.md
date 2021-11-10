# Changelog

`FSharp.Data.Migrations` is a simple database migration utility library.

All notable changes to this project will be documented in this file.

## [Unreleased]

- Logger
  - A Verbose `MigrationsOption` for more logging output.
- Add the ability to run `up` and `down` migrations
  - The default should be `up` only.
  - Identified in a script by @tags in a SQL comment (case insensitive)
    - `--- @UP` or multiline `/** @Up **/`
    - `--- @DOWN` or multiline `/** @Down **/`
  - Parse the script file for `up` and `down` identifier comments
- Possibly change the project name and `namespace`.

## [0.1.2] - 2021-11-10

### Added

- Logger
  - Coloured console output.
  - Made the `log writer` a `LogWriter` object.

## [0.1.2] - 2021-11-9

### Added

- A FAKE build script to build the project.

## [0.1.1] - 2021-11-08

### Fixed

- Fixed a logic bug with the `TransactionScope` implementation.

## [0.1.0] - 2021-11-06

### Added

- Initial project release.
- Supports `PostgreSQL` byt default.
- All `MigrationOption` details are set for `PostgreSQL`.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
