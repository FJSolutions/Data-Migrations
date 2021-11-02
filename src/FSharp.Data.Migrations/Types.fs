namespace FSharp.Data.Migrations

open System.Data

type public TransactionScope =
| PerScript
| PerRun
| NoTransaction

type MigrationConfiguration = {
  ScriptFolder: string
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
  
  /// <summary>Records a migration in the database</summary>
  /// <returns>The SQL to return when a migration has been run and must be recorded in the migrations table.</returns>
  abstract member RecordMigration : MigrationConfiguration -> string

  