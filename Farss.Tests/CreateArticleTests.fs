module CreateArticleTests

open Expecto
open Domain
open System

[<Tests>]
let tests = testList "Create article" [
        testList "Guid" [
            let invalidCases = [
                ("Cannot be null", null)
                ("Cannot be empty string", "")
                ("Cannot be only whitespace", "   ")
                ("Cannot be only whitespace tabs", "\t")
            ]

            let testInvalidArticleGuid (name, value) =
                testCase name <| fun _ ->
                    let result = ArticleGuid.create value
                    Expect.isError result "Should be invalid null or empty or whitespace"

            yield! List.map testInvalidArticleGuid invalidCases

            let validCases = [
                ("A valid example", "a globally unique identifier in string form")
            ]

            let testValidArticleGuid (name, value) =
                testCase name <| fun _ ->
                    let result = ArticleGuid.create value
                    Expect.isOk result "Should be a valid article guid"

            yield! List.map testValidArticleGuid validCases
        ]

        testList "timestamp" [
            testCase "None is invalid" <| fun _ ->
                let result = ArticleTimestamp.create None
                Expect.isError result "Should be required"

            testCase "Maps Some to Ok" <| fun _ ->
                let expected = DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.Zero)
                let result = ArticleTimestamp.create (Some expected)
                Expect.equal result (Ok expected) "Should map Some x to Ok x"
        ]

        testList "Article" [
            testCase "Create" <| fun _ ->
                let subscriptionId = Guid.NewGuid()
                let timestamp = DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero)
                
                let actual = 
                    Article.create 
                        "title" 
                        "guid" 
                        subscriptionId
                        "content" 
                        timestamp

                Expect.notEqual actual.Id Guid.Empty "Should create article id"
                Expect.equal actual.Title "title" "Title"
                Expect.equal actual.Guid "guid" "Guid"
                Expect.equal actual.Subscription subscriptionId "Subscription id"
                Expect.equal actual.Content "content" "Content"
                Expect.equal actual.PublishedAt timestamp "Timestamp"
                Expect.isFalse actual.IsRead "IsRead"
        ]
    ]

    
module Result =
    /// Traverse with early return on error
    let traverseE (items: Result<_, _> list) =   
        let rec impl items acc =
            match items with
            | [] -> Ok acc
            | Ok r::tail -> impl tail (r::acc)
            | Error e::_ -> Error e
        impl items [] |> Result.map List.rev

    /// Traverse and accumulate all errors. Greedy
    let traverse (items: Result<_, _> list) =
        let rec impl items acc =
            match acc, items with
            | acc, []  -> acc
            | Ok acc, Ok r::tail -> impl tail (Ok (r::acc))
            | Ok _, Error e::tail -> impl tail (Error [e])
            | Error acc, Ok _::tail -> impl tail (Error acc)
            | Error acc, Error e::tail -> impl tail (Error (e::acc))
        impl items (Ok []) 
            |> Result.map List.rev 
            |> Result.mapError List.rev

[<Tests>]
let traverseTests =
    testList "Result" [
        testList "Result.traverseE" [
            testCase "Flips list of results to result of list" <| fun _ -> 
                let result = Result.traverseE []

                Expect.equal result (Ok [])  "Flips L of Rs to R of L"

            testCase "Accumulates oks" <| fun _ -> 
                let result = Result.traverseE [Ok 1; Ok 2]

                Expect.equal result (Ok [1; 2]) "Accumulates values"
            
            testCase "Aborts at first error" <| fun _ ->
                let result = Result.traverseE [ Ok 1; Error "a"; Ok 2; Error "b" ]

                Expect.equal result (Error "a") "Halts eagerly on first error"
        ]
        testList "Result.traverse" [
            testCase "Flips list of results to result of list" <| fun _ -> 
                let result = Result.traverse []

                Expect.equal result (Ok [])  "Flips L of Rs to R of L"

            testCase "Accumulates oks" <| fun _ -> 
                let result = Result.traverse [Ok 1; Ok 2]

                Expect.equal result (Ok [1; 2]) "Accumulates values"
            
            testCase "Accumulates errors" <| fun _ ->
                let result = Result.traverse [ Ok 1; Error "a"; Ok 2; Error "b" ]

                Expect.equal result (Error ["a"; "b"]) "Accumulates all errors"
        ]
    ]
    
