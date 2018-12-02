module Task

open FSharp.Control.Tasks.V2
open System.Threading.Tasks

let map f (t: Task<_>) = task {
    let! result = t
    return f result
} 
