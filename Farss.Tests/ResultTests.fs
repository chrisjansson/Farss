module ResultTests

open Expecto

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
    

