// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Npgsql
open FSharp.Data.Migrations

[<EntryPoint>]
let main argv =
  let config = Configuration.configure argv

  // Get the base configuration
  Migrator.configure ()
  
  // Migrator transaction scope option
  // |> Migrator.transactionScope Migrator.NoTransaction 
  
  // Set the migration action
  |> Migrator.migrationAction config.Action

  // Migrator read scripts
  |> Migrator.scriptsFolder config.MigrationsFolder

  // Set the migrations database connection
  |> Migrator.connection (
        match config.ConnectionString with 
        | Some c -> Some (new NpgsqlConnection(c))
        | None -> None
      )

  // Display run Result
  |> Migrator.run
  
  0 // return an integer exit code
