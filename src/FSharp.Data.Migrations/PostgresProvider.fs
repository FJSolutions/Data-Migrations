namespace FSharp.Data.Migrations

module sb = FSharp.Text.StringBuilder

type PostgreSQL () =
  interface IMigrationDbProvider with
    member __.CheckMigrationsTableExists options =
      sb.create ()
      |> sb.add "SELECT COUNT(*) FROM information_schema.tables "
      |> sb.addSomeF "WHERE table_schema = '%s' " options.DbSchema
      |> sb.addFIf options.DbSchema.IsSome "AND table_name = '%s' " options.DbTableName
      |> sb.addFIf (not options.DbSchema.IsSome) "WHERE table_name = '%s' " options.DbTableName
      |> sb.toString

    member __.CreateMigrationsTable options =
      sb.create ()
      |> sb.add "CREATE TABLE "
      |> sb.addSomeF "%s." options.DbSchema
      |> sb.add options.DbTableName
      |> sb.add "( script varchar(1024) NOT NULL PRIMARY KEY, created_at timestamp with time zone DEFAULT now() ); "
      |> sb.toString
          
    member __.GetMigrations options =
      sb.create ()
      |> sb.add "SELECT script FROM "
      |> sb.addSomeF "%s." options.DbSchema
      |> sb.add options.DbTableName
      |> sb.add " ORDER BY script; " 
      |> sb.toString

    member __.AddUpMigration options =
      sb.create ()
      |> sb.add "INSERT INTO "
      |> sb.addSomeF "%s." options.DbSchema
      |> sb.add options.DbTableName
      |> sb.add " (script, created_at) VALUES(@ScriptName, now() ); "
      |> sb.toString
      
    member __.DeleteDownMigration options =
      sb.create ()
      |> sb.add "DELETE FROM "
      |> sb.addSomeF "%s." options.DbSchema
      |> sb.add options.DbTableName
      |> sb.add " WHERE script = @ScriptName; "
      |> sb.toString
