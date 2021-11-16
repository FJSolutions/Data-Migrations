module Configuration
  
  open Argu
  open dotenv.net
    
  type Arguments =
    | [<CustomAppSettings "DB_CONNECTION_STRING"; AltCommandLine "-c">] Connection_String of path:string
    | [<CustomAppSettings "MIGRATIONS_FOLDER"; AltCommandLine "-m";>] Migrations_Folder of path:string
    | [<CliPrefix(CliPrefix.None); First; Unique>] Up
    | [<CliPrefix(CliPrefix.None); First; Unique>] Down of count:uint
    | [<CliPrefix(CliPrefix.None); First; Unique>] List
    | [<CliPrefix(CliPrefix.None); First; Unique>] New of file_name:string
    interface IArgParserTemplate with 
      member self.Usage =
        match self with
        | Connection_String _ -> "The database connection string."
        | Migrations_Folder _ -> "Specifies the location the migration scripts folder."
        | Up _ -> "(default) Run all outstanding UP migrations"
        | Down _ -> "Revert <count> of run migrations"
        | List _ -> "Displays a list of the un-migrated scripts still in the migration folder"
        | New _ -> "Creates a new script file in the migrations folder"


  let configure argv =
    DotEnv.Load(DotEnvOptions(probeLevelsToSearch = 5, probeForEnv = false, ignoreExceptions = true, trimValues = true))

    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some System.ConsoleColor.Red)
    let parser = ArgumentParser.Create<Arguments>(programName = "migrate.exe", errorHandler = errorHandler)

    let options = parser.Parse (inputs = argv, configurationReader = EnvironmentVariableConfigurationReader ())

    let action = 
      match (options.TryGetResult List) with
      | Some _ -> FSharp.Data.Migrations.List
      | None ->
        match (options.TryGetResult Down) with
        | Some d -> FSharp.Data.Migrations.Down d
        | None -> 
          match (options.TryGetResult New) with
          | Some f -> FSharp.Data.Migrations.New f
          | None -> FSharp.Data.Migrations.Action.Up

    // Return a record with values or default's set
    {|
      ConnectionString = options.GetResult Connection_String
      MigrationsFolder = options.GetResult (Migrations_Folder, defaultValue = @"..\..\..\..\..\migrations")
      Action = action
    |}