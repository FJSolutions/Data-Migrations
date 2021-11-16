namespace FSharp.Data.Migrations

open System.IO
open System.Data
open ResultBuilder
open Internal

module internal DbScriptRunner =
  open System.Text.RegularExpressions

  let private upRegex = Regex("""(--|\/\*+[\s\*]+)\s*@UP\b""", RegexOptions.ECMAScript ||| RegexOptions.IgnoreCase ||| RegexOptions.Multiline ||| RegexOptions.Compiled)
  let private downRegex = Regex("""(--|\/\*+[\s\*]+)\s*@DOWN\b""", RegexOptions.ECMAScript ||| RegexOptions.IgnoreCase ||| RegexOptions.Multiline)

  let private readScriptFile (file:FileInfo) (isUpAction:bool) =
    try
      let sql = (file.OpenText ()).ReadToEnd ()

      // Identify UP and/or DOWN parts in the script
      let m = upRegex.Match sql
      let upPos = if m.Success then m.Index else -1
      
      let m = downRegex.Match sql
      let downPos = if m.Success then m.Index else -1
      
      // Extract up or down parts of the script
      match upPos,downPos with
      | -1, -1 -> Ok sql
      | n, -1 when n >= 0 -> Ok sql
      | -1, n when n >= 0 -> Error $"The SQL for '{file.Name}' contains a DOWN section but no UP section"
      | u, d -> 
            if isUpAction then
              Ok (sql.Substring(u, d - u))
            else
              Ok (sql.Substring d)
      | _ -> Error ("The SQL statement has UP or DOWN sections defined!\n" + sql)
    with | e ->
      Error <| sprintf "Error reading script file (%s): %s" file.Name e.Message


  let private executeMigration (options:MigrationConfiguration) sql = 
    try
      if options.Connection.Value.State = ConnectionState.Closed then
        options.Connection.Value.Open ()

      let cmd = options.Connection.Value.CreateCommand ()
      cmd.CommandText <- sql
      cmd.ExecuteNonQuery () |> ignore
      Ok true
    with | e ->
      Error e.Message

  let private transactMigration (options:MigrationConfiguration) sql =
    if options.Connection.Value.State = ConnectionState.Closed then
        options.Connection.Value.Open ()

    let transaction = options.Connection.Value.BeginTransaction ()

    match (executeMigration options sql) with
    | Ok i -> 
              transaction.Commit ()
              Ok i
    | Error e -> 
              transaction.Rollback ()
              Error e
  
  let private processMigration (options:MigrationConfiguration) sql = 
    if options.TransactionScope.TransactionPerScript () then
      transactMigration options sql
    else
      executeMigration options sql

  let private readAndExecuteUpMigration (options:MigrationConfiguration) (logger: Logger) (file:FileInfo) = 
    result {
      // Read the contents of the script file
      let! sql = readScriptFile file options.Action.isUpAction

      // Execute script 
      let! success = processMigration options sql

      if success then
        // Record it in migrations table
        let! _ = 
          if options.Action.isUpAction then
            logger.success (sprintf "Ran UP migration: %s" file.Name)
            DbRunner.addUpMigration options file.Name
          else
            logger.success (sprintf "Ran DOWN migration: %s" file.Name)
            DbRunner.removeDownMigration options file.Name

        return true
      else
        return! Error "An error occurred!"
    }

  let runMigrations (options:MigrationConfiguration) (logger: Logger) (scriptFiles:FileInfo list) : Result<_, string> =
    // Internal function to recurse the file list and execute the scripts
    let rec loop (files:FileInfo list) lastResult =
      match files, lastResult with
      | _, Error _ -> lastResult
      | [], _ -> lastResult
      | [file], _ -> loop [] <| readAndExecuteUpMigration options logger file
      | file::tl, _ -> loop tl <| readAndExecuteUpMigration options logger file
      
    // Start a transaction if the scope is `PerRun`
    let tran = 
      if options.TransactionScope.TransactionPerRun () then
        if options.Connection.Value.State = ConnectionState.Closed then
          options.Connection.Value.Open ()

        Some (options.Connection.Value.BeginTransaction ())
      else
        None
    
    // Call the file list
    let result = loop scriptFiles (Ok true)

    // Process the transaction
    match tran with
    | None -> ()
    | Some t -> 
        match result with 
        | Ok _ -> t.Commit ()
        | Error _ -> t.Rollback ()

    // Return the result
    result
