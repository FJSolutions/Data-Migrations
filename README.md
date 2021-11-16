# FSharp.Data.Migrations

`FSharp.Data.Migrations` is a lightweight database migration `tool` that is designed to be added to a project's `dotnet` tool chain.

It currently supports `PostgreSQL` connections for database migrations.

## Installation

If a `tool-manifest` does not yet already exist then create one, and install the tool

```sh
dotnet new tool-manifest

dotnet tool install --add-source <path-to-nupkg> migrate
```

**N.B.** The Project is currently not registered on `NuGet` and so must be built from source using the `fake` script in the project root which outputs a `Nuget` package to the `./nupkg/` folder:

```sh
dotnet fake run ./build.fsx --target nuget
```



## Init

In a new project folder the tool can be used to setup the migrations folder and a skeleton of the `.env` file.

```sh
dotnet migrate init
```

This will create a `migrations` folder in the current working directory and an `.env` file with the following two keys defined:

```text
CONNECTION_STRING=
MIGRATIONS_FOLDER=.\migrations\
```

The database connection string must be supplied (`.env` files should never be committed to source control for security reasons).

## New

A new migration script template file could then be created using the tool:

```sh
dotnet migrate new "Create Users table"
```

Which would create a new—timestamp prefixed—`.sql` file of this name (with spaces and illegal characters removed) in the migrations folder. The file simply has placeholders for the `UP` and `DOWN` script sections that will be run. By default a script is considered to be an `UP` only script unless it has both of these sections in. The `@Up` and `@Down` comments support both styles of SQL comment, but they must be the first non-comment character in the comment.

## UP

This is the default action and will create a `migrations` table if it doesn't exist yet, and run the `UP` sections of the scripts that haven't been executed yet.

```sh
dotnet migrate up
```



## Down

The `down` command takes a number of recorded scripts to revert. It gets the list of scripts to revert and reads the `@DOWN` section from them and then executes them in reverse order.

```sh
dotnet migrate down 1
```

## Help

All the configuration and command line options can be seen by running the `--help` switch.

```sh
dotnet migrate --help
```

