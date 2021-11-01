// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Npgsql
open FSharp.Data.Migrations

[<EntryPoint>]
let main argv =
  let config = Configuration.configure argv

  // Get the base configuration
  Migrator.configure
  
  // Migrator find scripts folder
  // |> Migrator.transactionScope Migrator.NoTransaction 

  // Migrator read scripts
  // |> Migrator.scriptsFolder "./migrations"

  // Display run Result
  |> Migrator.run (new NpgsqlConnection(config.ConnectionString))
  
  |> printfn "Got Run result: %A"
  
  0 // return an integer exit code