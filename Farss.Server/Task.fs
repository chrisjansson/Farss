module Task

open FSharp.Control.Tasks.V2
open System.Threading.Tasks

let map f (t: Task<_>) = task {
    let! result = t
    return f result
} 

let traverse (tasks: Task<_> list) = 
    Task.WhenAll(tasks)
    |> map List.ofArray
    
    //let rec inner tasks acc = task {
    //        match tasks with
    //        | [] -> 
    //            return acc
    //        | head::tail ->
    //            let! r = head
    //            let acc = r::acc
    //            return! inner tail acc
    //    }
    //inner tasks []