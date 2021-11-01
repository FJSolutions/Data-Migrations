namespace FSharp.Data.Migrations

open System.Data
open System.IO

module Migrator =

  type public TransactionScope =
    | PerScript
    | PerRun
    | NoTransaction

  type MigrationConfiguration = {
    ScriptFolder: string
    TransactionScope: TransactionScope
  }

  ///<summary>Creates a default configuration record.</summary>
  let configure =
    { 
      ScriptFolder = Internal.normalizePath "../../../../../migrations"
      TransactionScope = PerScript
    }
  
  /// <summary>
  /// Sets the folder to look for SQL migration scripts in.
  /// (The default is a `migrations` folder in the project root)
  /// </summary>
  let scriptsFolder folder (options: MigrationConfiguration) =
    { options with ScriptFolder = (Internal.normalizePath folder) }
    
  /// <summary>Sets the scope of transactions for when the migration scripts are run</summary>
  let transactionScope scope (options: MigrationConfiguration) =
    { options with TransactionScope = scope }
    
  ///<summary>Runs the migrations on the supplied database connection using the supplied migration options.</summary>
  let run (connection: IDbConnection) (options: MigrationConfiguration) : Result<_, string> =
    ResultBuilder.result {
      // use connection 

      // Verify the scripts folder exists
      let! result = Internal.checkScriptFolderExists options.ScriptFolder

      // Migrator read scripts
      let! result = Internal.getScriptFiles result

      // TODO: Migrator ensure migrations table exists & check what has been run

      // TODO: Create execution list

      // TODO: Execute script and record it in migrations table

      // TODO: On error, rollback and stop executing scripts

      // TODO: Return run status

      // connection.Dispose ()

      return result
    } 
    
