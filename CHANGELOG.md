# Changelog

`FSharp.Data.Migrations` is a simple database migration utility library.

All notable changes to this project will be documented in this file.

## [Unreleased]

- An `info` command
  - To show the final settings in the configuration record.
  - To show all the run migration scripts from the database.
  - To list the contents of the `migrations` folder (and which ones have been run).

## [0.5.1] - 2021-11-16

### Fixed

- A layout bug in the script template

## [0.5.0] - 2021-11-16

### Added

- An `init` command
  - To create the default migration folder (if it doesn't exist)
  - To create the `.env` file it it doesn't exist
    - To add the `migrations` folder as a parameter to the `.env` file
    - To add the database connection to the `.env` file.
- An `info` command
- Made the fluid configuration more resilient when chaining

## [0.4.1] - 2021-11-16

### Fixed

- Renamed the `Console` application to `Migrate` and set `migrate` as the main tool command.

### Added

- Packaged the console application as a `dotnet` command `tool`.

## [0.4.0] - 2021-11-13

### Added

- A `new` command to create a script file template in the `migrations` folder.

## [0.3.0] - 2021-11-12

### Fixed

- A logic bug that enabled you to try and migrate down more scripts than had been migrated up

### Added

- `list` command that shows the number of un-run migrations.

## [0.2.0] - 2021-11-12

### Added

- Migrations
  - Made upwards migrations the default
  - Added the ability to run a selected number of downward migrations.
  - Scripts parses for `up` and `down` identifier comment tags (case insensitive)
    - `--- @UP` or multiline `/** @Up **/`
    - `--- @DOWN` or multiline `/** @Down **/`

## [0.1.3] - 2021-11-11

### Added

- Logger
  - Redesigned the logger to be purely functional.

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
