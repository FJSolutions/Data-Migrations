namespace FSharp.Data.Migrations

open System
open System.Data
open System.IO

module Migrator =
  let inline private printFinalMessage (logger:Internal.Logger) result =
    match result with
    | Ok _ -> logger.title "\nSuccessfully ran migrations.\n"
    | Error e -> logger.error e

  ///Creates a default configuration record.
  let configure () =
    { 
      LogWriter = Console.Out
      ScriptFolder = Internal.normalizePath @".\migrations\"
      ScriptFilterPattern = "*.sql"
      Connection = None
      TransactionScope = PerScript
      Database = PostgreSQL()
      DbSchema = Some "public"
      DbTableName = "migrations"
      Action = Up
    } : MigrationConfiguration
  
  /// Sets the action to perform (UP or DOWN).
  /// (The default is `Up`)
  let migrationAction (action:FSharp.Data.Migrations.Action) (options:MigrationConfiguration) =
    { options with Action = action }
  
  let connection (con:IDbConnection option) (options:MigrationConfiguration) =
    { options with Connection = con }

  let outputWriter (writer: TextWriter) (options:MigrationConfiguration) =
    { options with LogWriter = writer }
  
  /// Sets the folder to look for SQL migration scripts in.
  /// (The default is a `migrations` folder in the project root, i.e. four levels from the folder where the Entry Assembly is executing)
  let scriptsFolder (folder: string option) (options: MigrationConfiguration) =
    match folder with 
    | None -> options
    | Some f -> { options with ScriptFolder = (Internal.normalizePath f) }

  /// Sets the file filter patter for files in the migration scripts folder
  /// (The default is `*.sql`)
  let scriptsFileFilter filterPattern (options: MigrationConfiguration) =
    { options with ScriptFilterPattern = filterPattern }
    
  /// Sets the scope of transactions for when the migration scripts are run.
  /// (The default is `transaction per script`)
  let transactionScope scope ( options: MigrationConfiguration ) =
    { options with TransactionScope = scope }

  /// Sets the default database schema
  /// (The default is `public`)
  let dbSchema schema (options: MigrationConfiguration) =
    { options with DbSchema = schema}

 /// Sets the name of the migrations table where executed scripts are stored
 /// (The default is `migrations)
  let dbTableName tableName (options: MigrationConfiguration) =
    { options with DbTableName = tableName}
    
  ///Runs the migrations on the supplied database connection using the supplied migration options.
  let run (options: MigrationConfiguration) =
    let logger = Internal.createLogger options.LogWriter
    logger.title "\n# Running Migrations\n"
    
    let result = 
      match options.Action with
      // Initializes the resources needed to run migrations
      | Init -> ActionRunner.init options logger
      // Migrate UP
      | Up -> ActionRunner.up options logger
      // Migrate DOWN
      | Down n -> ActionRunner.down options logger n
      // List un-run migration scripts
      | List -> ActionRunner.list options logger
      // Create a new script template file
      | New fileName -> ActionRunner.newScript options logger fileName
      // Displays all the configuration and migration informations
      | Info -> Error "Not yet implemented!"
      
    // Cleanup the database connection
    match options.Connection with
    | Some con -> con.Dispose ()
    | _ -> ()

    // Prints any errors that were generated
    printFinalMessage logger result
