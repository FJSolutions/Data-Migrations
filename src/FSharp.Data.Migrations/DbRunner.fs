namespace FSharp.Data.Migrations

open System.Data

module DbRunner =
  let runCheckMigrationsTableExists (options:MigrationConfiguration) (con:IDbConnection) : Result<bool, string> =
    try
      let sql = options.Database.CheckMigrationsTableExists options
      
      if con.State = ConnectionState.Closed then
        con.Open ()

      let cmd = con.CreateCommand ()
      cmd.CommandText <- sql

      let reader  = cmd.ExecuteReader CommandBehavior.CloseConnection
      reader.Read () |> ignore
      let result = ((reader.GetInt32 0) > 0)
      reader.Close ()

      Ok result
    with | e -> 
      Error e.Message

  let runCreateMigrationsTable (options:MigrationConfiguration) (con:IDbConnection) : Result<bool, string> =
    try
      let sql = options.Database.CreateMigrationsTable

      if con.State = ConnectionState.Closed then
        con.Open ()

      let cmd = con.CreateCommand ()
      cmd.CommandText <- sql.ToString ()
      Ok (cmd.ExecuteNonQuery () > 0)
    with |e ->
      Error e.Message


  let runGetMigrations (options:MigrationConfiguration) (con:IDbConnection) : Result<string list, string> =
    try
      let sql = options.Database.GetMigrations options

      if con.State = ConnectionState.Closed then
        con.Open ()

      let cmd = con.CreateCommand ()
      cmd.CommandText <- sql.ToString ()
      let reader = cmd.ExecuteReader CommandBehavior.CloseConnection

      let rec readScripts (reader:IDataReader) (list: string list) =
        if reader.Read () then
          readScripts reader ((reader.GetString 0) :: list)
        else
          reader.Close ()
          list

      Ok (readScripts reader [])
    with |e ->
      Error e.Message

  let runRecordMigration (options:MigrationConfiguration) (con:IDbConnection) (scriptName:string) : Result<bool, string> =
    try
      let sql = options.Database.RecordMigration options

      if con.State = ConnectionState.Closed then
        con.Open ()
      
      let cmd = con.CreateCommand ()
      cmd.CommandText <- sql

      let param = cmd.CreateParameter ()
      param.ParameterName <- "@ScriptName"
      param.DbType <- DbType.String
      param.Value <- scriptName
      cmd.Parameters.Add param |> ignore

      Ok (cmd.ExecuteNonQuery () > 0)
    with |e ->
      Error e.Message

