module AsyncResult

let mapResult f ar: Async<Result<_, _>> = async {
    let! r = ar
    return Result.map f r
}

let bind f ar: Async<Result<_, _>> = async {
    let! r = ar
    return Result.bind f r
}

let Return (r: Result<_,_>) = async.Return r

let map f =
    f |> Result.map |> Async.map

