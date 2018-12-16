[<AutoOpen>]
module ResultBuilder

type ResultBuilder() =
    member x.Bind(comp, func) = Result.bind func comp
    member x.Return(value) = Ok value

let result = ResultBuilder()