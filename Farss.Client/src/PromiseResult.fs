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