namespace FSharp.Text

module StringBuilder =
  open System.Text

  /// Creates and returns a new `StringBuilder
  let create () = 
    StringBuilder ()

  let add<'a> (value:'a) (sb:StringBuilder) = 
    sb.Append value
  
  let addSome<'a> (option:Option<'a>) (sb:StringBuilder) = 
    match option with
    | Some v -> sb.Append v
    | None -> sb
    
  let addNone<'a> (option:Option<'a>) (value:'a) (sb:StringBuilder) = 
    match option with
    | Some _ -> sb
    | None -> sb.Append value
  
  let addf<'a> (format:Printf.StringFormat<'a -> string>) (value:'a) (sb:StringBuilder) = 
    sb.Append (sprintf format value)
  
  let addFIf<'a> (condition:bool) (format:Printf.StringFormat<'a -> string>) (value:'a) (sb:StringBuilder) = 
    if condition then
      addf format value sb
    else
      sb

  let addSomeF<'a> (format:Printf.StringFormat<'a -> string>) (option:Option<'a>) (sb:StringBuilder) = 
    match option with
    | Some v -> sb.Append (sprintf format v)
    | None -> sb
  
  let addNoneF<'a> (format:Printf.StringFormat<'a -> string>) (option:Option<'a>) (value:'a) (sb:StringBuilder) = 
    match option with
    | Some _ -> sb
    | None -> sb.Append (sprintf format value)
    
  let toString (sb:StringBuilder) = 
    let s = sprintf ".%s" ""
    sb.ToString ()
