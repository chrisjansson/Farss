module AsyncResult

let mapResult f ar: Async<Result<_, _>> = async {
    let! r = ar
    return Result.map f r
}

let bind f ar: Async<Result<_, _>> = async {
    let! r = ar
    return Result.bind f r
}

let bindAsync (f: _ -> Async<_>) ar: Async<Result<_, _>> = async {
    let! r = ar
    match r with
    | Ok r ->
        let! r = f r
        return Ok r
    | Error e -> return Error e
}

let Return (r: Result<_,_>) = async.Return r

let map f =
    f |> Result.map |> Async.map

