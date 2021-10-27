module Configuration

open Argu
open dotenv.net

type Arguments =
  | [<CustomAppSettings "DB_CONNECTION_STRING"; AltCommandLine "-wd">] Connection_String of path:string

  interface IArgParserTemplate with 
    member self.Usage =
      match self with
      | Connection_String _ -> "specify the database connection string."

let configure argv =
  do DotEnv.Load(DotEnvOptions(trimValues = true, probeLevelsToSearch = 1))

  let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some System.ConsoleColor.Red)
  let parser = ArgumentParser.Create<Arguments>(programName = "console.exe", errorHandler = errorHandler)


  let options = parser.Parse (inputs = argv, configurationReader = EnvironmentVariableConfigurationReader ())

  // Return a record with values or default's set
  {|
    ConnectionString = options.GetResult Connection_String
  |}
