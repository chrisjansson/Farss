module SpecTests

open Expecto
open SubscribeToFeedSpecs

[<Tests>]
let tests = 
    specs "Spec infrastructure" [
        specCaseAsync "In memory server" <| fun f -> async {
            use client = f.CreateClient()
            let! response = client |> HttpClient.getAsync "/ping"
            response.EnsureSuccessStatusCode() |> ignore
        }
    ]
