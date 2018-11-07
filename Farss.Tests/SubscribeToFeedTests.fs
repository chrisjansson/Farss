module SubscribeToFeedTests

open Expecto
open SubscribeToFeedWorkflow
open FeedReaderAdapter
open System

module FakeFeedReaderAdapter =
    let stubResult result: FeedReaderAdapter = {  getFromUrl = fun _ -> result }

    let parseError (message: string) =
        Exception(message) |> ParseError  |> Error |> AsyncResult.Return

    let fetchError (message: string) =
        Exception(message) |> FetchError  |> Error |> AsyncResult.Return


    let feed (feed: CodeHollow.FeedReader.Feed) =
        feed |> Ok  |> AsyncResult.Return

module Expect =
    let equalAsync actual expected message = async {
        let! a = actual
        let! e = expected
        Expect.equal a e message
    }

[<Tests>]
let tests = testList "subscribe to feed tests" [
        let cases = [
            "fails subscribe when fetching fails", fun r -> async {
                let fetchResult = FakeFeedReaderAdapter.fetchError "fetch error"
                let expected = AsyncResult.mapResult ignore fetchResult
                let adapter = FakeFeedReaderAdapter.stubResult fetchResult

                let result = subscribeToFeed adapter r { Url = "any url" }

                do! Expect.equalAsync result expected "should fail with feed adapter result"
            }

            "fails subscribe when feed parsing fails", fun r -> async {
                let fetchResult = FakeFeedReaderAdapter.parseError "parse error"
                let expected = AsyncResult.mapResult ignore fetchResult
                let adapter = FakeFeedReaderAdapter.stubResult fetchResult

                let result = subscribeToFeed adapter r { Url = "any url" }

                do! Expect.equalAsync result expected "should fail with feed adapter result"
            }

            "result is ok when successful", fun r -> async {
                let fetchResult = FakeFeedReaderAdapter.feed (CodeHollow.FeedReader.Feed())
                let adapter = FakeFeedReaderAdapter.stubResult fetchResult

                let! result = subscribeToFeed adapter r { Url = "any url" }

                Expect.isOk result "should return ok when successful"
            }

            "saves feed when successful", fun r -> async {
                let fetchResult = FakeFeedReaderAdapter.feed (CodeHollow.FeedReader.Feed())
                let adapter = FakeFeedReaderAdapter.stubResult fetchResult

                do! subscribeToFeed adapter r { Url = "any url" } |> Async.Ignore

                let expected = { Domain.Feed.Url = "any url" }
                Expect.equal (r.getAll()) [ expected ] "should save feed"
            }
        ]

        let createTest (name, f) =
            testAsync name {
                let repository = Persistence.create ()
                do! f repository
            }

        yield! List.map createTest cases
    ]