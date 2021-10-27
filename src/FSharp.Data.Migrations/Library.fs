namespace FSharp.Data.Migrations

open System.Data
open System.IO
open System.Reflection

module Migrator =

  type TransactionScope =
    | PerScript
    | PerRun
    | NoTransaction

  type MigrationConfiguration = {
    ScriptFolder: string
    TransactionScope: TransactionScope
  }

  ///<summary>Creates a default configuration record.</summary>
  let configure =
    let options = { 
      ScriptFolder = "./scripts"
      TransactionScope = PerScript
    }

    (Assembly.GetEntryAssembly ()).Location
    |> Directory.GetParent
    |> printfn "%O"
    
    options
  
  /// <summary>Sets the folder to look for SQL migration scripts in.</summary>
  let scriptsFolder folder (options: MigrationConfiguration) =
    { options with ScriptFolder = folder }
    
  /// <summary>Sets the scope of transactions for when the migration scripts are run</summary>
  let transactionScope scope (options: MigrationConfiguration) =
    { options with TransactionScope = scope }
    
  ///<summary>Runs the migrations on the supplied database connection using the supplied migration options.</summary>
  let run (connection: IDbConnection) (options: MigrationConfiguration) : Result<unit, string> =
    // TODO: Verify the scripts folder exists 

    // TODO: Migrator read scripts

    // TODO: Migrator ensure migrations table exists & check what has been run

    // TODO: Create execution list

    // TODO: Execute script and record it in migrations table

    // TODO: On error, rollback and stop executing scripts

    // TODO: Display run status

    Error "'Run' Not Implemented"