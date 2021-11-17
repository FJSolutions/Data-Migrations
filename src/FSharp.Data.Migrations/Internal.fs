namespace FSharp.Data.Migrations

open System
open System.IO

module internal Internal =

  type Logger = {
    title: string -> unit
    info: string -> unit
    success: string -> unit
    error: string -> unit
  }

  let normalizePath (path:string) =
    let basePath = 
      match Path.IsPathRooted path with
      | true -> path
      | false -> 
          Path.Combine [| (Directory.GetCurrentDirectory ()); path |]

    Path.GetFullPath basePath

  let inline checkScriptFolderExists (scriptsFolder:string) (logger:Logger) (create:bool) =
    match (Directory.Exists scriptsFolder) with
    | true -> Ok (DirectoryInfo scriptsFolder)
    | false -> 
        if create then
          logger.info $"Created migrations folder: {scriptsFolder}"
          Ok (Directory.CreateDirectory scriptsFolder)
        else
          Error $"The scripts folder (%s{scriptsFolder}) doesn't exist!"

  let getScriptFiles (filter:string) (folder:DirectoryInfo) : Result<FileInfo list, string> =
    folder.GetFiles(filter)
    |> Array.toList
    |> List.sortBy (fun f -> f.Name)
    |> Ok


  let createLogger (writer:TextWriter) =
    let writer = if not (isNull writer) then writer else TextWriter.Null
    let logger (writer:TextWriter) (colour:ConsoleColor) (prefix:string) (message:string) =
      Console.ForegroundColor <- colour
      writer.WriteLine (prefix + message)
      Console.ResetColor ()

    {
      title = logger writer ConsoleColor.White String.Empty
      info = logger writer ConsoleColor.Cyan "[Info] "
      success = logger writer ConsoleColor.DarkGreen "[Success] "
      error = logger writer ConsoleColor.Red "[Error] "
    }