namespace FSharp.Data.Migrations

open System
open System.IO
open System.Reflection

module internal Internal =

  let normalizePath (path:string) =
    let (+/) path1 path2 = Path.Combine [| path1; path2|]
    let basePath = 
      match Path.IsPathRooted path with
      | true -> path
      | false -> 
          let p = (Assembly.GetEntryAssembly ()).Location
                  |> Directory.GetParent
                  |> string
          p +/ path

    Path.GetFullPath basePath

  let inline checkScriptFolderExists path =
    match (Directory.Exists path) with
    | true -> Ok (DirectoryInfo path)
    | false -> Error $"The scripts folder (%s{path}) doesn't exist!"

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