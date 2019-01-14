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

    let throwsAsyncT op message = async {
        let mutable opFailed = false    
        try
            do! (op ())
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
    open System.Globalization

    let specs name tests = 
        testList name tests |> testSequencedGroup "integration tests"

    let testFixtureAsync (setup: 'a -> Async<unit>) (tests: (string * 'a) seq) =
        seq {
            for t in tests do
                let test = setup (snd t)
                yield testCaseAsync (fst t) test
        }

    
    let testCodeWIthCulture (testCode: TestCode) (culture: CultureInfo) = 
        let setCulture culture = 
            CultureInfo.CurrentCulture <- culture
            CultureInfo.CurrentUICulture <- culture
            //CultureInfo.DefaultThreadCurrentCulture <- culture
            //CultureInfo.DefaultThreadCurrentUICulture <- culture

        let captureCulture _ =
            let currentCulture = CultureInfo.CurrentCulture
            let currentUiCulture = CultureInfo.CurrentUICulture
            //let defaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentCulture
            //let defaultThreadCurrentUiCulture = CultureInfo.DefaultThreadCurrentUICulture
            (currentCulture, currentUiCulture) //, defaultThreadCurrentCulture, defaultThreadCurrentUiCulture)

        let resetCulture (currentCulture, currentUiCulture) = //, defaultThreadCurrentCulture, defaultThreadCurrentUiCulture) =
            CultureInfo.CurrentCulture <- currentCulture
            CultureInfo.CurrentUICulture <- currentUiCulture
            //CultureInfo.DefaultThreadCurrentCulture <- defaultThreadCurrentCulture
            //CultureInfo.DefaultThreadCurrentUICulture <- defaultThreadCurrentUiCulture

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

    let testWithCultures (cultures: CultureInfo list) test =
        let replacer label (testCode: TestCode) =
            let tests = 
                cultures 
                |> List.map (fun c ->  TestLabel(c.Name, TestCase(testCodeWIthCulture testCode c, Normal), Normal))
            testList label tests
        Expecto.Test.replaceTestCode replacer test

    let testCultureInvariance test =
        let cultures = [ CultureInfo.GetCultureInfo("sv-SE"); CultureInfo.GetCultureInfo("en-US") ]
        testWithCultures cultures test