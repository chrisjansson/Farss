module TaskResult

open System.Threading.Tasks
open FSharp.Control.Tasks

let map f =        
    f |> Result.map |> Task.map

let bind f (ar: Task<Result<_,_>>): Task<Result<_, _>> = task {
    let! r = ar
    return Result.bind f r
}

let bindTask (f: 'a -> Task<Result<'b, _>>)(ar: Task<Result<'a,_>>): Task<Result<_, _>> = task {
    let! r = ar
    
    return!
        match r with
        | Ok r -> f r
        | Error e -> Task.FromResult(Error e)
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