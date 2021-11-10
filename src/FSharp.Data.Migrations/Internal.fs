namespace FSharp.Data.Migrations

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


  let createLogWriter (writer:TextWriter) =
    let writer = if not (isNull writer) then writer else TextWriter.Null
    new LogWriter(writer)