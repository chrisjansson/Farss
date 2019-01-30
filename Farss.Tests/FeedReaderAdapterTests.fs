module FeedReaderAdapterTests

open Expecto
open FeedBuilder
open TestStartup
open FeedReaderAdapter
open System

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

            //TODO: add test with a missing link, it's required by code hollow parser

            "Parses feed item", fun (f: InMemoryFeedReader) -> async {
                let content = 
                    feed "title" 
                    |> withItem (
                        feedItem2 "item 1" 
                        |> withId "a guid" 
                        |> withContent "content for item 1" 
                        |> withPublishingDate (DateTimeOffset(2001, 3, 2, 12, 1, 2, TimeSpan.Zero))
                        |> withUpdatedDate (DateTimeOffset(2002, 3, 2, 12, 1, 2, TimeSpan.Zero))
                        |> withLink "http://alink")
                    |> withItem (
                        feedItem2 "item 2" 
                        |> withId "item 2 guid" 
                        |> withContent "content for item 2"
                        |> withUpdatedDate (DateTimeOffset(2000, 3, 2, 12, 1, 2, TimeSpan.Zero))
                        |> withLink "http://another_link")
                    |> toAtom

                f.Add("url", content)
            
                let! result = f.Adapter.getFromUrl "url"
                let unboxed = unbox result
                Expect.equal unboxed.Items.Length 2 "Feed articles"
                Expect.equal 
                    unboxed.Items 
                    [
                        { Item.Title = "item 1"; Id = "a guid"; Content = "content for item 1"; Timestamp = Some (DateTimeOffset(2002, 3, 2, 12, 1, 2, TimeSpan.Zero)); Link = Some "http://alink/" }
                        { Item.Title = "item 2"; Id = "item 2 guid"; Content = "content for item 2"; Timestamp = Some(DateTimeOffset(2000, 3, 2, 12, 1, 2, TimeSpan.Zero)); Link = Some "http://another_link/" } 
                    ] 
                    "Feed items"
            }
        ]

        let setup test =
            let reader = createInMemoryFeedReader()
            test reader

        yield! testFixtureAsync setup tests
    ]
    |> testCultureInvariance