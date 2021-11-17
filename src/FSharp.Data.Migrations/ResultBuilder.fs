namespace FSharp.Data.Migrations

module ResultBuilder =
  open System

  let bind f m = Result.bind f m

  let (>>=) m f = bind f m

  type ResultBuilder () =
    member __.Return x = Ok x

    member __.ReturnFrom (m:Result<_,_>) = m

    member __.Bind (m, f) = bind f m

    member __.Zero () = Ok ()

    member __.TryFinally (body, f) =
      try
        __.ReturnFrom (body ())
      finally
        f () 

    member __.Using (disposable:IDisposable) f =
      __.TryFinally(f, disposable.Dispose)

    member __.Delay (f) = f ()

  let result = ResultBuilder()
