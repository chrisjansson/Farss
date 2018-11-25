module FakeFeedReaderAdapter

open FeedReaderAdapter
open System

let stubResult result: FeedReaderAdapter = {  getFromUrl = fun _ -> result }

let parseError (message: string) =
    Exception(message) |> ParseError  |> Error |> AsyncResult.Return

let fetchError (message: string) =
    Exception(message) |> FetchError  |> Error |> AsyncResult.Return 

type FeedReaderAdapterStub = 
    {
        SetResult: string * Result<FeedReaderAdapter.Feed, FeedReaderAdapter.FeedError> -> unit
        Adapter: FeedReaderAdapter.FeedReaderAdapter
    }

let createStub ():FeedReaderAdapterStub  =
    let mutable results = Map.empty

    let setResult (url, result) =
        results <- Map.add url result results
        
    let getFromUrl url =
        async.Return (Map.find url results)

    let adapter: FeedReaderAdapter.FeedReaderAdapter = 
        {
            FeedReaderAdapter.FeedReaderAdapter.getFromUrl = getFromUrl
        }
    {
        SetResult = setResult
        Adapter = adapter
    }