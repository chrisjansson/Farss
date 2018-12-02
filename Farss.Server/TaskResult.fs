module TaskResult

open FSharp.Control.Tasks.V2
open System.Threading.Tasks
    
let map f =        
    f |> Result.map |> Task.map

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