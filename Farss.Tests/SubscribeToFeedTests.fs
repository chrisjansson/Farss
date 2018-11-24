module SubscribeToFeedTests

open Expecto
open SubscribeToFeedWorkflow
open FeedReaderAdapter
open System
open Persistence

let expectBadRequest actual message = async {
        let! actual' = actual
        match actual' with        
        | Error (BadRequest (m, e)) ->
            Expect.equal m message "Bad request message"
            Expect.equal m message "Bad request message"
        | _ -> Tests.failtest "Expected bad request"
    }

type FeedProjection = { Url: string }

let project (feed: Domain.Subscription): FeedProjection =
    { Url = feed.Url }

[<Tests>]
let tests = testList "subscribe to feed tests" [
        let cases = [
            "fails subscribe when fetching fails", fun r -> async {
                let fetchResult = FakeFeedReaderAdapter.fetchError "fetch error"
                let adapter = FakeFeedReaderAdapter.stubResult fetchResult

                let result = subscribeToFeed adapter r { Url = "any url" }

                do! expectBadRequest result "fetch error"
            }

            "fails subscribe when feed parsing fails", fun r -> async {
                let fetchResult = FakeFeedReaderAdapter.parseError "parse error"
                let adapter = FakeFeedReaderAdapter.stubResult fetchResult

                let result = subscribeToFeed adapter r { Url = "any url" }

                do! expectBadRequest result "parse error"
            }

            "result is ok when successful", fun r -> async {
                let fr = TestStartup.createInMemoryFeedReader()
                let xml =FeedBuilder.feedItem "item" |> FeedBuilder.toRss 
                fr.Add("any url", xml)

                let! result = subscribeToFeed fr.Adapter r { Url = "any url" }

                Expect.isOk result "should return ok when successful"
            }

            "saves feed when successful", fun r -> async {
                let fr = TestStartup.createInMemoryFeedReader()
                let xml =FeedBuilder.feedItem "item" |> FeedBuilder.toRss 
                fr.Add("any url", xml)

                do! subscribeToFeed fr.Adapter r { Url = "any url" } |> Async.Ignore

                let expected = { FeedProjection.Url = "any url" }
                Expect.equal (r.getAll() |> List.map project) [ expected ] "should save feed"
            }

            "created feed has non empty guid", fun r -> async {
                let fr = TestStartup.createInMemoryFeedReader()
                let xml =FeedBuilder.feedItem "item" |> FeedBuilder.toRss 
                fr.Add("any url", xml)

                do! subscribeToFeed fr.Adapter r { Url = "any url" } |> Async.Ignore

                Expect.all (r.getAll()) (fun f -> f.Id <> Guid())  "all feeds should have non empty guid ids"
            }
        ]

        let createTest (name, f) =
            testAsync name {
                let repository = Persistence.create ()
                do! f repository
            }

        yield! List.map createTest cases
    ]
