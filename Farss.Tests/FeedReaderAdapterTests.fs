module FeedReaderAdapterTests

open Expecto
open FeedBuilder
open TestStartup

let unbox r =
    match r with
    | Ok v -> v
    | Error _ -> failwith "error"
    
[<Tests>]
let tests =
    testList "Feed reader adapter" [
        let tests = [
            "Test", fun (f: InMemoryFeedReader) -> async {
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
        ]

        let setup test =
            let reader = createInMemoryFeedReader()
            test reader

        yield! testFixtureAsync setup tests
    ]