// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Npgsql
open FSharp.Data.Migrations
open System.Data

[<EntryPoint>]
let main argv =
  let results = Configuration.configure argv

  // Get the base configuration
  Migrator.configure
  
  // Migrator find scripts folder
  |> Migrator.transactionScope NoTransaction
  |> printfn "Got parse result %O"

  // TODO: Migrator read scripts

  // TODO: Migrator ensure migrations table exists & check what has been run

  // TODO: Create execution list

  // TODO: Execute script and record it in migrations table

  // TODO: On error, rollback and stop executing scripts

  // TODO: Display run status
  
  0 // return an integer exit code