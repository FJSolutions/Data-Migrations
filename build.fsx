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
let version = "0.2.0"
let buildDir = "./build/"

// *** Define Targets ***
Target.create "Clean" (fun _ ->
  Trace.log " --- Cleaning stuff --- "
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
  let projFile = "src/Console/Console.fsproj"
  Xml.loadDoc(projFile) 
  |> Xml.replaceXPathInnerText "/Project/PropertyGroup/Version" version
  |> Xml.saveDoc projFile

  // Use the `dotnet` command line tool to build the project
  let setParams (defaults:DotNet.BuildOptions) = {
      defaults with 
        OutputPath = Some buildDir 
        Configuration = DotNet.BuildConfiguration.Release
    }
  DotNet.build setParams "src/Console/Console.fsproj" // buildDir "Build" 
)

// Dependencies
open Fake.Core.TargetOperators

"Clean"
  ==> "Copy Files"
  ==> "Build"

// start build
Target.runOrDefault "Build"
