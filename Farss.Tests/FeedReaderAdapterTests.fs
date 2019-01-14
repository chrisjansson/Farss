module FeedReaderAdapterTests

open Expecto
open FeedBuilder
open TestStartup
open FeedReaderAdapter
open System
open System.Globalization

let unbox r =
    match r with
    | Ok v -> v
    | Error e -> failwith (sprintf "%A" e)
    
let testCodeInCulture (testCode: TestCode) (culture: CultureInfo) = 
    let setCulture culture = 
        CultureInfo.CurrentCulture <- culture
        CultureInfo.CurrentUICulture <- culture
        CultureInfo.DefaultThreadCurrentCulture <- culture
        CultureInfo.DefaultThreadCurrentUICulture <- culture

    let captureCulture _ =
        let currentCulture = CultureInfo.CurrentCulture
        let currentUiCulture = CultureInfo.CurrentUICulture
        let defaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentCulture
        let defaultThreadCurrentUiCulture = CultureInfo.DefaultThreadCurrentUICulture
        (currentCulture, currentUiCulture, defaultThreadCurrentCulture, defaultThreadCurrentUiCulture)

    let resetCulture (currentCulture, currentUiCulture, defaultThreadCurrentCulture, defaultThreadCurrentUiCulture) =
        CultureInfo.CurrentCulture <- currentCulture
        CultureInfo.CurrentUICulture <- currentUiCulture
        CultureInfo.DefaultThreadCurrentCulture <- defaultThreadCurrentCulture
        CultureInfo.DefaultThreadCurrentUICulture <- defaultThreadCurrentUiCulture

    let wrap culture test arg =
        let initial = captureCulture ()
        setCulture culture

        try 
            test arg
        finally 
            resetCulture initial

    let wrapAsync culture test = async {
        let initial = captureCulture ()
        setCulture culture
        try 
            do! test
        finally 
            resetCulture initial
    }

    match testCode with
    | Sync stest ->
        Sync (wrap culture stest)
    | SyncWithCancel stest -> 
        SyncWithCancel (wrap culture stest)
    | Async atest ->
        Async (wrapAsync culture atest)
    | tc ->
        tc

let inCulture (cultures: CultureInfo list) test =
    let replacer label (testCode: TestCode) =
        let tests = cultures |> List.map (fun c ->  TestLabel(c.Name, TestCase(testCodeInCulture testCode c, Normal), Normal))
        testList label tests
    Expecto.Test.replaceTestCode replacer test

[<Tests>]
let tests =
    inCulture 
        [ CultureInfo.GetCultureInfo("sv-SE"); CultureInfo.GetCultureInfo("en-US") ]
        (testList "Feed reader adapter" [
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
                    failwith (CultureInfo.CurrentCulture.Name)
                }

                "Parses feed item", fun (f: InMemoryFeedReader) -> async {
                    let content = 
                        feed "title" 
                        |> withItem (
                            feedItem2 "item 1" 
                            |> withId "a guid" 
                            |> withContent "content for item 1" 
                            |> withPublishingDate (DateTimeOffset(2001, 3, 2, 12, 1, 2, TimeSpan.Zero))
                            |> withUpdatedDate (DateTimeOffset(2002, 3, 2, 12, 1, 2, TimeSpan.Zero)))
                        |> withItem (
                            feedItem2 "item 2" 
                            |> withId "item 2 guid" 
                            |> withContent "content for item 2"
                            |> withUpdatedDate (DateTimeOffset(2000, 3, 2, 12, 1, 2, TimeSpan.Zero)))
                        |> toAtom

                    f.Add("url", content)
            
                    let! result = f.Adapter.getFromUrl "url"
                    let unboxed = unbox result
                    Expect.equal unboxed.Items.Length 2 "Feed articles"
                    Expect.equal 
                        unboxed.Items 
                        [
                            { Item.Title = "item 1"; Id = "a guid"; Content = "content for item 1"; Timestamp = Some (DateTimeOffset(2002, 3, 2, 12, 1, 2, TimeSpan.Zero)) }
                            { Item.Title = "item 2"; Id = "item 2 guid"; Content = "content for item 2"; Timestamp = Some(DateTimeOffset(2000, 3, 2, 12, 1, 2, TimeSpan.Zero)) } 
                        ] 
                        "Feed items"
                    failwith (CultureInfo.CurrentCulture.Name)
                }
            ]

            let setup test =
                let reader = createInMemoryFeedReader()
                test reader

            yield! testFixtureAsync setup tests
        ])