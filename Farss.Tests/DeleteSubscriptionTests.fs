module DeleteSubscriptionTests

open System
open Expecto
open Persistence
open Domain

//todo: move to expecto extensions
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
                let command: Dto.DeleteSubscriptionDto = { Id = None }
                let result = DeleteSubscriptionWorkflow.deleteSubscription r command
                Expect.isError result "Should fail when id is not given"

            "fails with InvalidParameter when no id is given", fun r -> 
                let command: Dto.DeleteSubscriptionDto = { Id = None }
                let result = DeleteSubscriptionWorkflow.deleteSubscription r command
                Expect.expectInvalidParameter result

            "deletes subscription", fun r ->
                let subscription: Subscription = { Id = Guid.NewGuid(); Url = "some url"; Title = "title" }
                let subscription2: Subscription = { Id = Guid.NewGuid(); Url = "another some url"; Title = "title" }
                r.save subscription
                r.save subscription2

                let command: Dto.DeleteSubscriptionDto = { Id = Some subscription.Id }
                let result = DeleteSubscriptionWorkflow.deleteSubscription r command
                
                Expect.isOk result "should delete feed ok"
                Expect.equal (r.getAll()) [subscription2] "should delete one feed"
        ]

        let createTest (name, f) =
            test name {
                let repository = Persistence.create ()
                f repository
            }

        yield! List.map createTest cases
    ]