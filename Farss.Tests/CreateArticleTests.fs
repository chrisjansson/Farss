module CreateArticleTests

open Expecto
open Domain

[<Tests>]
let tests = testList "Create article tests" [
        testList "Article Guid" [
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
    ]
