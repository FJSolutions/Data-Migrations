namespace FSharp.Data.Migrations

open System.IO

/// The scope in which the migation files should be executed
type public TransactionScope =
  | PerScript
  | PerRun
  | NoTransaction
  member __.TransactionPerScript () =
    match __ with
    | PerScript -> true
    | _ -> false
  member __.TransactionPerRun () =
    match __ with
    | PerRun -> true
    | _ -> false

/// The kind of migration action to perform 
type Action =
  | Up
  | Down of count:uint
  | List
  | New of name:string
  member self.isUpAction with get() =
    match self with
    | Up -> true
    | _ -> false
  member self.isDownAction with get() =
    match self with
    | Down _ -> true
    | _ -> false

/// The migration configuration record 
type MigrationConfiguration = {
  LogWriter: TextWriter 
  ScriptFolder: string
  ScriptFilterPattern: string
  TransactionScope: TransactionScope
  Database: IMigrationDbProvider
  DbSchema: string option
  DbTableName: string
  Action: Action
}
and
  /// Provides the interface for database specific Migration database providers
  IMigrationDbProvider =
    /// <summary>Checks if the migrations table exists</summary>
    /// <returns>The SQL to run for the provider to check of the migrations table exists</returns>
    abstract member CheckMigrationsTableExists : MigrationConfiguration -> string
    
    /// <summary>Creates the migrations table</summary>
    /// <returns>The SQL to run to create the migrations table.</returns>
    abstract member CreateMigrationsTable : MigrationConfiguration -> string
    
    /// <summary>Gets a list of all the migration scripts that have already been run successfully.</summary>
    /// <returns>The SQL to return to get all the migrations that have already been run.</returns>
    abstract member GetMigrations : MigrationConfiguration -> string
    
    /// <summary>
    /// Records an UP migration in the database.
    /// The parameter for the script file name must be called `@ScriptName`.
    /// </summary>
    /// <returns>The SQL to run when an UP migration has successfully been executed, to record the script in the migrations table.</returns>
    abstract member AddUpMigration : MigrationConfiguration -> string
    
    /// <summary>
    /// Removes a DOWN migration from the database.
    /// The parameter for the script file name must be called `@ScriptName`.
    /// </summary>
    /// <returns>The SQL to run when a DOWN migration has successfully executed, to remove the script in the migrations table.</returns>
    abstract member DeleteDownMigration : MigrationConfiguration -> string
