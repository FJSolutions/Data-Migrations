namespace FSharp.Data.Migrations


module Migrator =

  open System.Data

  ///<summary>Creates a default configuration record.</summary>
  let configure =
    { 
      ScriptFolder = Internal.normalizePath "../../../../../migrations"
      TransactionScope = PerScript
      Database = PostgreSQL()
      DbSchema = Some "public"
      DbMigrationsTableName = "migrations"
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

  let dbSchema schema (options: MigrationConfiguration) =
    { options with DbSchema = schema}

  let dbMigrationsTableName tableName (options: MigrationConfiguration) =
    { options with DbMigrationsTableName = tableName}
    
  ///<summary>Runs the migrations on the supplied database connection using the supplied migration options.</summary>
  let run (connection: IDbConnection) (options: MigrationConfiguration) : Result<_, string> =
    ResultBuilder.result {
      // Verify the scripts folder exists
      let! result = Internal.checkScriptFolderExists options.ScriptFolder

      // Read migration scripts
      let! scripts = Internal.getScriptFiles result

      // Ensure the migrations table exists & check what has been run
      let! result = DbRunner.runCheckMigrationsTableExists options connection

      // Create execution list
      let! result =
        if result then 
          Ok true
        else
          DbRunner.runCreateMigrationsTable options connection

      // Get the list of already executed scripts
      let! result = DbRunner.runGetMigrations options connection

      // TODO: Remove the existing scripts from the list
      let scripts = List.except result scripts 
                    |> List.map Internal.normalizePath

      // TODO: Loop the scripts

      // TODO: Execute script 
      
      // TODO: Record it in migrations table

      // TODO: On error, rollback and stop executing scripts

      // TODO: Return run status

      connection.Dispose ()

      return result
    } 
    
