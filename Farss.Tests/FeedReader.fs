module FeedReader

open Expecto
open CodeHollow.FeedReader
open System.IO

let readAsync = FeedReader.ReadAsync >> Async.AwaitTask

[<Tests>]
let tests =
    testList "FeedReader characterization tests" [
        testList "reading https feeds" [
            testAsync "read feed" {
                do! readAsync "https://codehollow.com/feed" |> Async.Ignore
            }
            testAsync "fails reading non feed" {
                let op = readAsync "https://wootaslkdj.com" |> Async.Ignore

                do! Expect.throwsAsync op "Throws on reading non rss feed"
            }
        ]

        testList "reading example feed" [
            testCase "stuff" <| fun _ ->
                let feedContent = File.ReadAllText("ExampleRssFeed.xml")
                let feed = FeedReader.ReadFromString(feedContent)
                Expect.equal feed.Title "Scripting News" "Title"
        ]
    ]
