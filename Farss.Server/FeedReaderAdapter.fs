module FeedReaderAdapter

open CodeHollow.FeedReader
open System

type AsyncResult<'T, 'E> = Async<Result<'T, 'E>>

module AsyncResult =
    let map f ar: Async<Result<_, _>> = async {
        let! r = ar
        return Result.map f r
    }

    let bind f ar: Async<Result<_, _>> = async {
        let! r = ar
        return Result.bind f r
    }

    let Return (r: Result<_,_>) = async.Return r

type  FeedError =
    | FetchError of Exception
    | ParseError of Exception

type FeedReaderAdapter = 
    {
        getFromUrl: string -> AsyncResult<Feed, FeedError>
    }

let createAdapter (): FeedReaderAdapter =
    //Todo: async catch
    let tryOrErrorAsync op errorConstructor arg = async {
        try
            let! result = op arg
            return Ok result
        with
        | e -> 
            return errorConstructor e |> Error
    }

    let tryOrError op errorConstructor arg =
        try 
            Ok (op arg)
        with
        | e -> Error (errorConstructor e)

    let fetch (url: string): AsyncResult<Feed, FeedError> = 
        let downloadBytesAsync (url: string) = Helpers.DownloadBytesAsync(url) |> Async.AwaitTask
        let tryDownloadBytesAsync (url: string) = tryOrErrorAsync downloadBytesAsync FetchError url
        let parseBytes (bytes: byte[]) = FeedReader.ReadFromByteArray(bytes)
        let tryParseBytes (bytes: byte[]) = tryOrError parseBytes ParseError bytes

        tryDownloadBytesAsync url |> AsyncResult.bind tryParseBytes

    {
        getFromUrl = fetch
    }
