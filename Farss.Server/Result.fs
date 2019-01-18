[<RequireQualifiedAccess>]
module Result

let tee (f: 'T -> unit) (r: Result<'T, _>)  =
    match r with
    | Ok o -> 
        f o
        r
    | _ -> r
let teeError (f: 'TError -> unit) (r: Result<_, 'TError>) =
    match r with
    | Error e -> 
        f e
        r
    | _ -> r

/// Traverse with early return on error
let traverseE (items: Result<_, _> list) =   
    let rec impl items acc =
        match items with
        | [] -> Ok acc
        | Ok r::tail -> impl tail (r::acc)
        | Error e::_ -> Error e
    impl items [] |> Result.map List.rev

/// Traverse and accumulate all errors. Greedy
let traverse (items: Result<_, _> list) =
    let rec impl items acc =
        match acc, items with
        | acc, []  -> acc
        | Ok acc, Ok r::tail -> impl tail (Ok (r::acc))
        | Ok _, Error e::tail -> impl tail (Error [e])
        | Error acc, Ok _::tail -> impl tail (Error acc)
        | Error acc, Error e::tail -> impl tail (Error (e::acc))
    impl items (Ok []) 
        |> Result.map List.rev 
        |> Result.mapError List.rev