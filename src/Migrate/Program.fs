// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Npgsql
open FSharp.Data.Migrations

[<EntryPoint>]
let main argv =
  let config = Configuration.configure argv

  // Get the base configuration
  Migrator.configure ()
  
  // Migrator find scripts folder
  // |> Migrator.transactionScope Migrator.NoTransaction 

  // Set the migration action
  |> Migrator.migrationAction config.Action

  // Migrator read scripts
  |> Migrator.scriptsFolder config.MigrationsFolder

  // Display run Result
  |> Migrator.run (new NpgsqlConnection (config.ConnectionString))
  
  0 // return an integer exit code
