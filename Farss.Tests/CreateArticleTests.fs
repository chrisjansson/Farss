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