namespace FSharp.Data.Migrations

open System
open System.IO

module internal Internal =

  let normalizePath (path:string) =
    let basePath = 
      match Path.IsPathRooted path with
      | true -> path
      | false -> 
          Path.Combine [| (Directory.GetCurrentDirectory ()); path |]

    Path.GetFullPath basePath

  let inline checkScriptFolderExists (options:MigrationConfiguration) =
    match (Directory.Exists options.ScriptFolder) with
    | true -> Ok (DirectoryInfo options.ScriptFolder)
    | false -> 
        match options.Action with 
        | Init -> Ok (Directory.CreateDirectory options.ScriptFolder)
        | _ -> Error $"The scripts folder (%s{options.ScriptFolder}) doesn't exist!"

  let getScriptFiles (filter:string) (folder:DirectoryInfo) : Result<FileInfo list, string> =
    folder.GetFiles(filter)
    |> Array.toList
    |> List.sortBy (fun f -> f.Name)
    |> Ok

  type Logger = {
    title: string -> unit
    info: string -> unit
    success: string -> unit
    error: string -> unit
  }

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