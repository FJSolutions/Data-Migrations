namespace FSharp.Data.Migrations

module internal ScriptTemplate =
  open System
  open System.IO
  open Internal

  let normalizeFileName (path:string) (name:string) =
    let MAX_LENGTH = 128
    let replaceChar c =
      if Array.contains c (Path.GetInvalidFileNameChars ()) then
        '_'
      elif Char.IsWhiteSpace c then
        '_'
      else
        c
    
    let fileName = String.map replaceChar name
    let fileName = sprintf "%s_%s" (DateTime.Now.ToString ("yyMMddHHmmss"))  fileName
    let fileName = if (String.length fileName) > MAX_LENGTH then 
                      fileName.Substring(0, MAX_LENGTH)
                    else 
                      fileName
    Path.Join [| path; (fileName + ".sql") |]

  let createScript (logger:Logger) (fullFileName:string) =
    let template = """
    /*************
    *   @UP
    *************/



    /***************
    *   @DOWN 
    ****************/

    
    """

    try
      File.WriteAllText (fullFileName, template)

      logger.success ("Created script template: "  + Path.GetFileName fullFileName)

      Ok true
    with 
    | e -> Error e.Message