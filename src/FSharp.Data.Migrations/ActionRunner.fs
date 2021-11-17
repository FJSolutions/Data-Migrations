namespace FSharp.Data.Migrations

open System.IO
open FSharp.Data.Migrations.ResultBuilder
open System

module internal ActionRunner =

  let init (options:MigrationConfiguration) (logger:Internal.Logger) =
    result {
      // Check and create the migrations folder
      let! _ = Internal.checkScriptFolderExists options.ScriptFolder logger true

      // Check the `.env` file exists & create it if it doesn't with the connection string key in it
      let envFile = Path.Combine [| Directory.GetCurrentDirectory (); ".env" |]
      return! match File.Exists envFile with
              | true -> Ok true
              | false ->
                  use file = File.CreateText envFile
                  file.WriteLine "CONNECTION_STRING="
                  file.WriteLine ("MIGRATIONS_FOLDER=" + options.ScriptFolder)
                  logger.info "Initialized the `.env` file"
                  Ok true
    }

  let getScripts (options:MigrationConfiguration) (logger:Internal.Logger) =
    result {
      // Verify the scripts folder exists
      let! result = Internal.checkScriptFolderExists options.ScriptFolder logger false

      // Read migration scripts
      let! scripts = Internal.getScriptFiles options.ScriptFilterPattern result
      
      // Ensure the migrations table exists & check what has been run
      let! result = DbRunner.runCheckMigrationsTableExists options
      
      // Create the migrations table if it doesn't exist list
      let! _ =
        if result then 
          Ok true
        else
          DbRunner.runCreateMigrationsTable options
      
      
      // Get the list of already executed scripts
      let! result = DbRunner.getRunMigrationsList options

      return scripts, result
    }

  let up (options:MigrationConfiguration) (logger:Internal.Logger) =
    result {
      let! scripts, result =  getScripts options logger

      // Remove the existing scripts from the list
      let scripts = List.filter (fun (r:FileInfo) -> not (List.exists (fun f -> r.Name = f) result)) scripts 
      logger.info (sprintf "%i script(s) were found to migrate up" (List.length scripts))
      
      // Loop the UP scripts
      return! DbScriptRunner.runMigrations options logger scripts
    }

  let down (options:MigrationConfiguration) (logger:Internal.Logger) (count:uint) =
    result {
      let! scripts, result =  getScripts options logger

      let scripts = 
            List.filter (fun (r:FileInfo) -> List.exists (fun f -> r.Name = f) result) scripts 
            |> List.rev
          
      let len = 
        match (List.length scripts), (Convert.ToInt32 count) with
        | sl, tn when sl < tn -> sl
        | _, tn -> tn
      let scripts = List.take len scripts
      logger.info (sprintf "%i script(s) were found to migrate down" len)

      // Loop the DOWN scripts
      return! DbScriptRunner.runMigrations options logger scripts
    }

  let list (options:MigrationConfiguration) (logger:Internal.Logger) =
    result {
      let! scripts, result =  getScripts options logger
      

      let scripts = List.filter (fun (r:FileInfo) -> not (List.exists (fun f -> r.Name = f) result)) scripts 
      let _ = match List.length scripts with
              | 0 -> 
                  logger.info "There are no un-run migration scripts"
              | n -> 
                  logger.info (sprintf "%i script(s) were found to still migrate:" n)
                  List.map (fun (f:FileInfo) -> logger.info ("    " + f.Name)) scripts |> ignore

      return true
    }

  let newScript (options:MigrationConfiguration) (logger:Internal.Logger) (fileName:string) =
    ScriptTemplate.normalizeFileName options.ScriptFolder fileName
    |> ScriptTemplate.createScript logger