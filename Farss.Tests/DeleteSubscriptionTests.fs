module DeleteSubscriptionTests

open System
open Expecto
open Persistence

module Expect =
    let expectInvalidParameter (result: Result<_,WorkflowError>) =
        match result with 
        | Error (InvalidParameter _) -> ()
        | _ -> Tests.failtest "Expected invalid parameter"

[<Tests>]
let tests = 
    testList "delete subscription tests"  [
        let cases = [
            "fails when no id is given", fun r -> 
                let command: Dto.DeleteSubscriptionDto = { Id = Nullable() }
                let result = DeleteSubscriptionWorkflow.deleteSubscription r command
                Expect.isError result "Should fail when id is not given"

            "fails with InvalidParameter when no id is given", fun r -> 
                let command: Dto.DeleteSubscriptionDto = { Id = Nullable() }
                let result = DeleteSubscriptionWorkflow.deleteSubscription r command
                Expect.expectInvalidParameter result
        ]

        let createTest (name, f) =
            test name {
                let repository = Persistence.create ()
                f repository
            }

        yield! List.map createTest cases
    ]