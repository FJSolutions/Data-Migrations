namespace FSharp.Data.Migrations

module ResultBuilder =

  let bind f m = Result.bind f m

  let (>>=) m f = bind f m

  type ResultBuilder() =
    member __.Return x = Ok x

    member __.ReturnFrom (m:Result<_,_>) = m

    member __.Bind(m, f) = bind f m

    member __.Zero = Ok ()

  let result = ResultBuilder()
