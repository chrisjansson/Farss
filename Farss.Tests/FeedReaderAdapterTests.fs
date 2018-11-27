module FeedReaderAdapterTests

open Expecto
open FeedBuilder
open TestStartup
open FeedReaderAdapter

let unbox r =
    match r with
    | Ok v -> v
    | Error _ -> failwith "error"
    
[<Tests>]
let tests =
    testList "Feed reader adapter" [
        let tests = [
            "Parses feed", fun (f: InMemoryFeedReader) -> async {
                let content = 
                    feed "title" 
                    |> withDescription2 "description"
                    |> toRss2

                f.Add("url", content)
            
                let! result = f.Adapter.getFromUrl "url"
                let unboxed = unbox result
                Expect.equal unboxed.Title "title" "Feed title"
                Expect.equal unboxed.Description "description" "Feed description"
            }

            "Parses feed item", fun (f: InMemoryFeedReader) -> async {
                let content = 
                    feed "title" 
                    |> withItem (feedItem2 "item 1" |> withId "a guid")
                    |> withItem (feedItem2 "item 2")
                    |> toRss2

                f.Add("url", content)
            
                let! result = f.Adapter.getFromUrl "url"
                let unboxed = unbox result
                Expect.equal unboxed.Items.Length 2 "Feed articles"
                Expect.equal unboxed.Items [ { Item.Title = "item 1"; Id = "a guid" }; { Item.Title = "item 2"; Id = null } ] "Feed items"
            }
        ]

        let setup test =
            let reader = createInMemoryFeedReader()
            test reader

        yield! testFixtureAsync setup tests
    ]