[<RequireQualifiedAccess>]
module PromiseResult

let map f p = promise {
    let! res = p
    return 
        match res with
        | Ok v -> Ok (f v)
        | Error e -> Error e
}

let bind f p = promise {
    let! res = p
    match res with
    | Ok v -> return! f v
    | Error e -> return Error e
}

open Fable.Core.JS

let bindResult (f: 'r -> Result<_, 'e>) (p: Promise<Result<'r, 'e>>) = promise {
    let! res = p
    match res with
    | Ok v -> return f v
    | Error e -> return Error e
}

let resultEnd onSuccess onError p =
    promise {
        let! value = p
        match value with
        | Ok r ->
            onSuccess r
            ()
        | Error e ->
            onError e
            ()
    }
    