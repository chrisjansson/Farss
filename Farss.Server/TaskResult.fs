module TaskResult

open System.Threading.Tasks
open FSharp.Control.Tasks.V2

let map f =        
    f |> Result.map |> Task.map

let bind f (ar: Task<Result<_,_>>): Task<Result<_, _>> = task {
    let! r = ar
    return Result.bind f r
}

let tee f (t: Task<_>) = task {
    let! result = t
    return 
        match result with
        | Ok value ->
            f value
            result
        | _ ->
            result
}