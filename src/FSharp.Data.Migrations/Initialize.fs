namespace FSharp.Data.Migrations

module internal Initialize =
  open System.IO
  let init (options:MigrationConfiguration) (logger:Internal.Logger) =
    
    // Check the `.env` file exists & create it if it doesn't with the connection string key in it
    let envFile = Path.Combine [| Directory.GetCurrentDirectory (); ".env" |]
    match File.Exists envFile with
    | true -> Ok true
    | false ->
        use file = File.CreateText envFile
        file.WriteLine "CONNECTION_STRING="
        file.WriteLine ("MIGRATIONS_FOLDER=" + options.ScriptFolder)
        logger.info "Initialized the `.env` file"
        Ok true
