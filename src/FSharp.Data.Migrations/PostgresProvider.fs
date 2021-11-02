namespace FSharp.Data.Migrations
open System.Data
open System.Text

type PostgreSQL () =
  
  interface IMigrationDbProvider with
    member __.CheckMigrationsTableExists options =
      let sql = "SELECT COUNT(*) FROM information_schema.tables "
      match options.DbSchema with 
      | Some _ -> sql + "WHERE table_schema = @SchemaName AND table_name = @TableName;"
      | None -> sql + "WHERE table_name = @TableName; "

    member __.CreateMigrationsTable options =
        let sql = StringBuilder()
        sql.Append "CREATE TABLE " |> ignore
        if options.DbSchema.IsSome then
          sql.Append options.DbSchema.Value |> ignore
          sql.Append "." |> ignore
        sql.Append options.DbMigrationsTableName |> ignore  
        sql.Append "( script varchar(1024) NOT NULL, created_at timestamp with time zone DEFAULT now() ) " |> ignore

        sql.ToString ()

          
    member __.GetMigrations options =
      let sql = StringBuilder()
      sql.Append "SELECT script FROM " |> ignore
      if options.DbSchema.IsSome then
        sql.Append options.DbSchema.Value |> ignore
        sql.Append "." |> ignore
      sql.Append options.DbMigrationsTableName |> ignore  
      sql.Append " ORDER BY script" |> ignore

      sql.ToString ()


    member __.RecordMigration options =
      failwith "Not implemented yet"
