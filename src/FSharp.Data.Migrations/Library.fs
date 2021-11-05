namespace FSharp.Data.Migrations

open System
open System.IO


module Migrator =

  open System.Data

  ///<summary>Creates a default configuration record.</summary>
  let configure () =
    { 
      LogWriter = Console.Out
      ScriptFolder = Internal.normalizePath "../../../../../migrations"
      TransactionScope = PerScript
      Database = PostgreSQL()
      DbSchema = Some "public"
      DbMigrationsTableName = "migrations"
    }
  
  /// Sets the output stream for writing migration messages to.
  /// (The default is `stdout`)
  let outputWriter (writer: TextWriter) (options:MigrationConfiguration) =
    { options with LogWriter = writer }
  
  /// <summary>
  /// Sets the folder to look for SQL migration scripts in.
  /// (The default is a `migrations` folder in the project root)
  /// </summary>
  let scriptsFolder folder (options: MigrationConfiguration) =
    { options with ScriptFolder = (Internal.normalizePath folder) }
    
  /// <summary>Sets the scope of transactions for when the migration scripts are run</summary>
  let transactionScope scope (options: MigrationConfiguration) =
    { options with TransactionScope = scope }

  let dbSchema schema (options: MigrationConfiguration) =
    { options with DbSchema = schema}

  let dbMigrationsTableName tableName (options: MigrationConfiguration) =
    { options with DbMigrationsTableName = tableName}
    
  ///<summary>Runs the migrations on the supplied database connection using the supplied migration options.</summary>
  let run (connection: IDbConnection) (options: MigrationConfiguration) : Result<_, string> =
    let writer = Internal.logWriter options.LogWriter
    writer "\n# Running Migrations\n"
    
    ResultBuilder.result {
      // Verify the scripts folder exists
      let! result = Internal.checkScriptFolderExists (options.ScriptFolder)
      writer (sprintf "A migration-scripts folder was found at: '%O'" result)

      // Read migration scripts
      let! scripts = Internal.getScriptFiles result

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
      writer (sprintf "%i script(s) were found to run" (List.length scripts))
      
      // Loop the scripts
      let! result = DbScriptRunner.runMigrations options connection writer scripts
      
      // Cleanup the database connection
      connection.Dispose ()
      writer "Successfully ran migrations."

      return result
    } 
    
