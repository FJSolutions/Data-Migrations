module Configuration

open Argu
open dotenv.net

type Arguments =
  | [<CustomAppSettings "DB_CONNECTION_STRING"; AltCommandLine "-c">] Connection_String of path:string
  | [<CustomAppSettings "MIGRATIONS_FOLDER"; AltCommandLine "-m";>] Migrations_Folder of path:string

  interface IArgParserTemplate with 
    member self.Usage =
      match self with
      | Connection_String _ -> "The database connection string."
      | Migrations_Folder _ -> "Specifies the location the migration scripts folder."

let configure argv =
  DotEnv.Load(DotEnvOptions(probeLevelsToSearch = 5, probeForEnv = false, ignoreExceptions = true, trimValues = true))

  let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some System.ConsoleColor.Red)
  let parser = ArgumentParser.Create<Arguments>(programName = "console.exe", errorHandler = errorHandler)


  let options = parser.Parse (inputs = argv, configurationReader = EnvironmentVariableConfigurationReader ())

  // Return a record with values or default's set
  {|
    ConnectionString = options.GetResult Connection_String
    MigrationsFolder = options.GetResult (Migrations_Folder, defaultValue = @"..\..\..\..\..\migrations")
  |}
