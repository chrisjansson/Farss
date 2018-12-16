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
