[<AutoOpen>]
module ExpectoExtensions

open Expecto

module Expect = 
    let throwsAsync op message = async {
        let mutable opFailed = false    
        try
            do! op
        with     
        | _ ->
            opFailed <- true 

        if not opFailed then do
            Tests.failtest <| sprintf "Should throw esxception: %s" message
    }

    let equalAsync actual expected message = async {
        let! a = actual
        let! e = expected
        Expect.equal a e message
    }

[<AutoOpen>]
module Tests =
    let specs name tests = 
        testList name tests |> testSequencedGroup "integration tests"

