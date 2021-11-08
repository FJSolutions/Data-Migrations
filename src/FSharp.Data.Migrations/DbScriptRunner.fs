namespace FSharp.Data.Migrations

open ResultBuilder
open System.IO
open System.Data

module DbScriptRunner =

  let private readScriptFile (file:FileInfo) =
    try
      let sql = (file.OpenText ()).ReadToEnd ()
      Ok sql
    with | e ->
      Error <| sprintf "Error reading script file (%s): %s" file.Name e.Message


  let private executeMigration (con:IDbConnection) sql = 
    try
      if con.State = ConnectionState.Closed then
        con.Open ()

      let cmd = con.CreateCommand ()
      cmd.CommandText <- sql
      cmd.ExecuteNonQuery () |> ignore
      Ok true
    with | e ->
      Error e.Message

  let private transactMigration (con:IDbConnection) sql =
    if con.State = ConnectionState.Closed then
        con.Open ()

    let transaction = con.BeginTransaction ()

    match (executeMigration con sql) with
    | Ok i -> 
              transaction.Commit ()
              Ok i
    | Error e -> 
              transaction.Rollback ()
              Error e
  
  let private processMigration (options:MigrationConfiguration) (con:IDbConnection) sql = 
    if options.TransactionScope.TransactionPerScript () then
      transactMigration con sql
    else
      executeMigration con sql

  open ResultBuilder

  let private readAndExecuteMigration (options:MigrationConfiguration) (con:IDbConnection) (writer: string -> unit) (file:FileInfo) = 
    result {
      // Read the contents of the script file
      let! sql = readScriptFile file

      // Execute script 
      let! success = processMigration options con sql

      if success then
        // Record it in migrations table
        let! _ = DbRunner.runRecordMigration options con file.Name

        writer (sprintf "Ran migration script: %s" file.Name)

        return true
      else
        return! Error "An error occurred!"
    }

  let runMigrations (options:MigrationConfiguration) (con:IDbConnection) (writer: string -> unit) (scriptFiles:FileInfo list) : Result<_, string> =
    // Internal function to recurse the file list and execute the scripts
    let rec loop (files:FileInfo list) lastResult =
      match files, lastResult with
      | _, Error _ -> lastResult
      | [], _ -> lastResult
      | [file], _ -> loop [] <| readAndExecuteMigration options con writer file
      | file::tl, _ -> loop tl <| readAndExecuteMigration options con writer file
      
    // Start a transaction if the scope is `PerRun`
    let tran = 
      if options.TransactionScope.TransactionPerRun () then
        if con.State = ConnectionState.Closed then
          con.Open ()

        Some (con.BeginTransaction ())
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
