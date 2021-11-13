namespace FSharp.Data.Migrations

open System
open System.Data
open System.IO

module Migrator =
  let inline private printError (writer:Internal.Logger) result =
    match result with
    | Ok _ -> ()
    | Error e -> writer.error e

  ///Creates a default configuration record.
  let configure () =
    { 
      LogWriter = Console.Out
      ScriptFolder = Internal.normalizePath @"..\..\..\..\..\migrations\"
      ScriptFilterPattern = "*.sql"
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
  let dbTableName tableName (options: MigrationConfiguration) =
    { options with DbTableName = tableName}
    
  ///Runs the migrations on the supplied database connection using the supplied migration options.
  let run (connection: IDbConnection) (options: MigrationConfiguration) =
    let logger = Internal.createLogger options.LogWriter
    logger.title "\n# Running Migrations\n"
    
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

      // Check the migration action to see what to do next
      let! result = 
        match options.Action with
        // Migrate UP
        | Up ->
          // Remove the existing scripts from the list
          let scripts = List.filter (fun (r:FileInfo) -> not (List.exists (fun f -> r.Name = f) result)) scripts 
          logger.info (sprintf "%i script(s) were found to migrate up" (List.length scripts))
          
          // Loop the UP scripts
          DbScriptRunner.runMigrations options connection logger scripts
        
        // Migrate DOWN
        | Down n -> 
          let scripts = 
            List.filter (fun (r:FileInfo) -> List.exists (fun f -> r.Name = f) result) scripts 
            |> List.rev
          
          let len = 
            match (List.length scripts), (Convert.ToInt32 n) with
            | sl, tn when sl < tn -> sl
            | _, tn -> tn
          let scripts = List.take len scripts
          logger.info (sprintf "%i script(s) were found to migrate down" len)

          // Loop the DOWN scripts
          DbScriptRunner.runMigrations options connection logger scripts

        // List un-run migration scripts
        | List ->
          let scripts = List.filter (fun (r:FileInfo) -> not (List.exists (fun f -> r.Name = f) result)) scripts 
          match List.length scripts with
          | 0 -> logger.info "There are no un-run migration scripts"
          | n -> 
            logger.info (sprintf "%i script(s) were found to still migrate:" n)
            List.map (fun (f:FileInfo) -> logger.info ("    " + f.Name)) scripts |> ignore

          Ok true

        // Create a new script template file
        | New fileName ->
          let fileName = ScriptTemplate.normalizeFileName options.ScriptFolder fileName
          
          logger.title "\nSuccessfully ran migrations.\n"
          ScriptTemplate.createScript logger fileName
      
      return result  
    } 
      
    // Cleanup the database connection
    connection.Dispose ()

    // Prints any errors that were generated
    printError logger result
