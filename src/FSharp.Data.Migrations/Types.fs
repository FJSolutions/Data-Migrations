namespace FSharp.Data.Migrations

open System.IO

type LogWriter(writer:TextWriter) =
  member __.title (msg:string) =
    writer.WriteLine msg

  member __.success (msg:string) =
    System.Console.ForegroundColor <- System.ConsoleColor.DarkGreen
    writer.WriteLine $"[Success] {msg}"
    System.Console.ResetColor ()  

  member __.info (msg:string) =
    System.Console.ForegroundColor <- System.ConsoleColor.Cyan
    writer.WriteLine $"[Info] {msg}"
    System.Console.ResetColor ()
  
  member __.error (msg:string) =
    System.Console.ForegroundColor <- System.ConsoleColor.Red
    writer.WriteLine $"[Error] {msg}"
    System.Console.ResetColor ()

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

type MigrationConfiguration = {
  LogWriter: TextWriter 
  ScriptFolder: string
  ScriptFilterPattern: string
  TransactionScope: TransactionScope
  Database: IMigrationDbProvider
  DbSchema: string option
  DbMigrationsTableName: string
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
    /// Records a migration in the database.
    /// The parameter for the script file name must be called `@ScriptName`.
    /// </summary>
    /// <returns>The SQL to run when a migration has successfully been executed, to record the script in the migrations table.</returns>
    abstract member RecordMigration : MigrationConfiguration -> string
