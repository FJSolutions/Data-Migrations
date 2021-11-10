namespace FSharp.Data.Migrations

open System
open System.Data
open System.IO


module Migrator =
  let inline private printError (writer:LogWriter) result =
    match result with
    | Ok _ -> ()
    | Error e -> writer.error e

  ///Creates a default configuration record.
  let configure () : MigrationConfiguration =
    { 
      LogWriter = Console.Out
      ScriptFolder = Internal.normalizePath @"..\..\..\..\..\migrations\"
      ScriptFilterPattern = "*.sql"
      TransactionScope = PerScript
      Database = PostgreSQL()
      DbSchema = Some "public"
      DbMigrationsTableName = "migrations"
    }
  
  /// Sets the output stream for writing migration messages to.
  /// (The default is `stdout`)
  let outputWriter (writer: TextWriter) (options:MigrationConfiguration) =
    { options with LogWriter = writer }
  
  /// Sets the folder to look for SQL migration scripts in.
  /// (The default is a `migrations` folder in the project root, i.e. four levels from the folder where the Entry Assembly is executing)
  let scriptsFolder folder (options: MigrationConfiguration) =
    { options with ScriptFolder = (Internal.normalizePath folder) }

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
  let dbMigrationsTableName tableName (options: MigrationConfiguration) =
    { options with DbMigrationsTableName = tableName}
    
  ///Runs the migrations on the supplied database connection using the supplied migration options.
  let run (connection: IDbConnection) (options: MigrationConfiguration) =
    let writer = Internal.createLogWriter options.LogWriter
    writer.title "\n# Running Migrations\n"
    
    let result = ResultBuilder.result {
      // Verify the scripts folder exists
      let! result = Internal.checkScriptFolderExists (options.ScriptFolder)
      // writer (sprintf "A migration-scripts folder was found at: '%O'" result)

      // Read migration scripts
      let! scripts = Internal.getScriptFiles options.ScriptFilterPattern result

      // Ensure the migrations table exists & check what has been run
      let! result = DbRunner.runCheckMigrationsTableExists options connection
      
      // Create execution list
      let! _ =
        if result then 
          Ok true
        else
          DbRunner.runCreateMigrationsTable options connection
          
      // Get the list of already executed scripts
      let! result = DbRunner.runGetMigrations options connection

      // Remove the existing scripts from the list
      let scripts = List.filter (fun (r:FileInfo) -> not (List.exists (fun f -> r.Name = f) result)) scripts 
      writer.info (sprintf "%i script(s) were found to run" (List.length scripts))
      
      // Loop the scripts
      let! result = DbScriptRunner.runMigrations options connection writer scripts
      
      // Cleanup the database connection
      connection.Dispose ()
      writer.title "\nSuccessfully ran migrations.\n"

      return result
    } 

    // Prints any errors that were generated
    printError writer result
