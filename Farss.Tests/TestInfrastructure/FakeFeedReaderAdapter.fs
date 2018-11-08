module FakeFeedReaderAdapter

open FeedReaderAdapter
open System

let stubResult result: FeedReaderAdapter = {  getFromUrl = fun _ -> result }

let parseError (message: string) =
    Exception(message) |> ParseError  |> Error |> AsyncResult.Return

let fetchError (message: string) =
    Exception(message) |> FetchError  |> Error |> AsyncResult.Return

let feed (feed: CodeHollow.FeedReader.Feed) =
    feed |> Ok  |> AsyncResult.Return

