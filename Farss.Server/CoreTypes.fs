[<AutoOpen>]
module CoreTypes

open System.Threading.Tasks

type AsyncResult<'T, 'E> = Async<Result<'T, 'E>>
type TaskResult<'T, 'E> = Task<Result<'T, 'E>>
            
type OperationResult<'T, 'TError> = Result<'T, OperationError<'TError>>

and OperationError<'TError> = 
    | OperationError of exn
    | InnerError of 'TError