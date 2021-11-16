#r "paket:
nuget Fake.IO.FileSystem 
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.Cli 
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.DotNet

// Properties
let version = "0.4.1"
let buildDir = "./build/"
let migrateProjPath = "src/Migrate/Migrate.fsproj"

// *** Define Targets ***
Target.create "Clean" (fun _ ->
  Trace.log " --- Cleaning build directory --- "
  Shell.cleanDir buildDir
)

Target.create "Copy Files" (fun _ ->
  !! "./migrations/*.sql"
  |> Shell.copyFilesWithSubFolder buildDir

  Shell.copyFile buildDir ".env"

  File.append (buildDir + ".env") [@"MIGRATIONS_FOLDER=..\migrations"]
)

Target.create "Build" (fun _ ->
  // Update the assembly version numbers
  let projFile = "src/FSharp.Data.Migrations/FSharp.Data.Migrations.fsproj"
  Xml.loadDoc(projFile) 
  |> Xml.replaceXPathInnerText "/Project/PropertyGroup/Version" version
  |> Xml.saveDoc projFile

  Xml.loadDoc(migrateProjPath) 
  |> Xml.replaceXPathInnerText "/Project/PropertyGroup/Version" version
  |> Xml.saveDoc migrateProjPath

  // Use the `dotnet` command line tool to build the project
  let setParams (defaults:DotNet.BuildOptions) = {
      defaults with 
        OutputPath = Some buildDir 
        Configuration = DotNet.BuildConfiguration.Debug
    }
  DotNet.build setParams "src/Migrate/Migrate.fsproj" // buildDir "Build" 
)

Target.create "Nuget" (fun _ ->
  Trace.log "Building project as a Nuget Package"
  let setParams (defaults:DotNet.PackOptions) = {
      defaults with 
        OutputPath = Some "./nupkg" 
        Configuration = DotNet.BuildConfiguration.Release
    }
  DotNet.pack setParams migrateProjPath
)

// Dependencies
open Fake.Core.TargetOperators

"Clean"
  ==> "Copy Files"
  ==> "Build" 

"Nuget"

// start build
Target.runOrDefault "Build"
