module FeedReader

open Expecto
open CodeHollow.FeedReader
open System.Threading.Tasks

let throwsAsync op message = async {
    let mutable opFailed = false    
    try
        do! op
    with     
    | _ ->
        opFailed <- true 

    if not opFailed then do
        Tests.failtest <| sprintf "Should throw esxception: %s" message
}

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

                do! throwsAsync op "Throws on reading non rss feed"
            }
        ]
    ]
