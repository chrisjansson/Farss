module FeedReaderAdapter

open CodeHollow.FeedReader
open System

type AsyncResult<'T, 'E> = Async<Result<'T, 'E>>

module AsyncResult =
    let mapResult f ar: Async<Result<_, _>> = async {
        let! r = ar
        return Result.map f r
    }

    let bind f ar: Async<Result<_, _>> = async {
        let! r = ar
        return Result.bind f r
    }

    let Return (r: Result<_,_>) = async.Return r

module Async =
    let map f a = async {
        let! a' = a
        return (f a')
    }

type  FeedError =
    | FetchError of Exception
    | ParseError of Exception

type FeedReaderAdapter = 
    {
        getFromUrl: string -> AsyncResult<Feed, FeedError>
    }

let createAdapter (): FeedReaderAdapter =
    let tryOrErrorAsync op errorConstructor arg = async {
        match! (Async.Catch (op arg)) with
        | Choice1Of2 r -> return Ok r
        | Choice2Of2 r -> return Error (errorConstructor r)
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
