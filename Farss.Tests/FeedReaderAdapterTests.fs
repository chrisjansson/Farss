﻿module FeedReaderAdapterTests

open Expecto
open FeedBuilder
open TestStartup
open FeedReaderAdapter

let unbox r =
    match r with
    | Ok v -> v
    | Error e -> failwith (sprintf "%A" e)
    
[<Tests>]
let tests =
    testList "Feed reader adapter" [
        let tests = [
            "Parses feed", fun (f: InMemoryFeedReader) -> async {
                let content = 
                    feed "title" 
                    |> withDescription2 "description"
                    |> toAtom

                f.Add("url", content)
            
                let! result = f.Adapter.getFromUrl "url"
                let unboxed = unbox result
                Expect.equal unboxed.Title "title" "Feed title"
            }

            "Parses feed item", fun (f: InMemoryFeedReader) -> async {
                let content = 
                    feed "title" 
                    |> withItem (feedItem2 "item 1" |> withId "a guid" |> withContent "content for item 1")
                    |> withItem (feedItem2 "item 2" |> withId "item 2 guid" |> withContent "content for item 2")
                    |> toAtom

                f.Add("url", content)
            
                let! result = f.Adapter.getFromUrl "url"
                let unboxed = unbox result
                Expect.equal unboxed.Items.Length 2 "Feed articles"
                Expect.equal 
                    unboxed.Items 
                    [
                        { Item.Title = "item 1"; Id = "a guid"; Content = "content for item 1" }
                        { Item.Title = "item 2"; Id = "item 2 guid"; Content = "content for item 2" } 
                    ] 
                    "Feed items"
            }
        ]

        let setup test =
            let reader = createInMemoryFeedReader()
            test reader

        yield! testFixtureAsync setup tests
    ]