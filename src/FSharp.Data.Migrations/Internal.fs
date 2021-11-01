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

  let checkScriptFolderExists (path:string) =
    match Directory.Exists path with
    | true -> Ok (DirectoryInfo path)
    | false -> Error $"The scripts folder (%s{path}) doesn't exist!"

  let getScriptFiles (folder:DirectoryInfo) =
    folder.GetFiles("*.sql")
    |> Array.toList
    |> List.sortBy (fun f -> f.Name)
    |> Ok
