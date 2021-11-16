module Configuration
  
  open Argu
  open dotenv.net
    
  type Arguments =
    | [<CliPrefix(CliPrefix.None); First; Unique>] Init
    | [<CliPrefix(CliPrefix.None); First; Unique>] List
    | [<CliPrefix(CliPrefix.None); First; Unique>] Up
    | [<CliPrefix(CliPrefix.None); First; Unique>] Down of count:uint
    | [<CliPrefix(CliPrefix.None); First; Unique>] New of file_name:string
    | [<CustomAppSettings "CONNECTION_STRING"; AltCommandLine "-c">] Connection_String of path:string
    | [<CustomAppSettings "MIGRATIONS_FOLDER"; AltCommandLine "-m";>] Migrations_Folder of path:string
    interface IArgParserTemplate with 
      member self.Usage =
        match self with
        | Init _ -> "Sets up the current folder for migrations."
        | List _ -> "Displays a list of the un-migrated scripts still in the migration folder"
        | Up _ -> "(default) Run all outstanding UP migrations"
        | Down _ -> "Revert <count> of run migrations"
        | New _ -> "Creates a new script file in the migrations folder"
        | Connection_String _ -> "The database connection string (.env: CONNECTION_STRING)."
        | Migrations_Folder _ -> "Specifies the location the migration scripts folder (.env: MIGRATIONS_FOLDER)."

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
          | None -> 
            match (options.TryGetResult Init) with
            | Some f -> FSharp.Data.Migrations.Init
            | None -> FSharp.Data.Migrations.Action.Up

    // Return a record with values or default's set
    {|
      ConnectionString = options.TryGetResult Connection_String
      MigrationsFolder = options.TryGetResult Migrations_Folder
      Action = action
    |}