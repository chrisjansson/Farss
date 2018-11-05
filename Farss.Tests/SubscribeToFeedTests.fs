module SubscribeToFeedTests

open Expecto
open SubscribeToFeedWorkflow
open FeedReaderAdapter
open System

let tests = testList "subscribe to feed tests" [
        testAsync "fails subscribe when fetching fails" {
            let expectedA = Exception("oops") |> FetchError  |> Error |> AsyncResult.Return
            let adapter: FeedReaderAdapter = {  getFromUrl = fun _ -> expectedA }
            let repository = Persistence.create ()

            let! result = subscribeToFeed adapter repository { Url = "any url" }
            let! expected = expectedA

            Expect.equal result expected "should fail with feed adapter result"
        }
    ]
