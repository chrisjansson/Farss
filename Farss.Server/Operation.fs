module Operation

open FSharp.Control.Tasks

let execAsync (op: 'a -> TaskResult<'b, 'c>)  (arg: 'a) = task {
    try 
        let! result = op arg
        return 
            match result with
            | Ok o -> Ok o
            | Error e -> Error (InnerError e)
    with exn ->
        return Error (OperationError exn)
}