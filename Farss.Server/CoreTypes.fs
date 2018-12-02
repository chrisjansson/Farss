[<AutoOpen>]
module CoreTypes

type AsyncResult<'T, 'E> = Async<Result<'T, 'E>>