namespace FSharp.Data.Migrations

open System.Data

module DbRunner =
  let runCheckMigrationsTableExists (options:MigrationConfiguration) : Result<bool, string> =
    match options.Connection with 
    | None -> 
        match options.Action with
        | Init -> Ok true
        | _ -> Error "A connection has not been defined"
    | Some con -> 
        try
          // let con = options.Connection.Value
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

  let runCreateMigrationsTable (options:MigrationConfiguration) : Result<bool, string> =
    match options.Connection with 
    | None -> 
        match options.Action with
        | Init -> Ok true
        | _ -> Error "A connection has not been defined"
    | Some con -> 
      try
        let sql = options.Database.CreateMigrationsTable options

        if con.State = ConnectionState.Closed then
          con.Open ()

        let cmd = con.CreateCommand ()
        cmd.CommandText <- sql.ToString ()
        Ok (cmd.ExecuteNonQuery () > 0)
      with |e ->
        Error e.Message


  let runGetMigrations (options:MigrationConfiguration) : Result<string list, string> =
    match options.Connection with 
    | None -> 
        match options.Action with
        | Init -> Ok []
        | _ -> Error "A connection has not been defined"
    | Some con -> 
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

  let addUpMigration (options:MigrationConfiguration) (scriptName:string) : Result<bool, string> =
    try
      let con = options.Connection.Value
      let sql = options.Database.AddUpMigration options

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

  let removeDownMigration (options:MigrationConfiguration) (scriptName:string) : Result<bool, string> =
    try
      let con = options.Connection.Value
      let sql = options.Database.DeleteDownMigration options

      if con.State = ConnectionState.Closed then
        con.Open ()
      
      let cmd = con.CreateCommand ()
      cmd.CommandText <- sql

      let param = cmd.CreateParameter ()
      param.ParameterName <- "@ScriptName"
      param.DbType <- DbType.String
      param.Value <- scriptName
      cmd.Parameters.Add param |> ignore

      Ok (cmd.ExecuteNonQuery () = 0)
    with |e ->
      Error e.Message

