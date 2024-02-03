module Farss.Tests.HttpTests

open System
open Expecto
open Farss.Server

[<Tests>]
let tests =
    testList "HTTP Client stuffs" [
        testCaseAsync "Abc" <| (task {

            //What does 304 return in body?

            // let! result = CachedHttpClient.doWork "https://www.fz.se/"
            // let! result = CachedHttpClient.doWork ("https://blog.ploeh.dk/atom.xml", None, DateTimeOffset.Parse("Fri, 02 Feb 2024 12:54:55 GMT") |> Some)
            let! result = CachedHttpClient.get ("https://blog.ploeh.dk/atom.xml", Some "\"65bce61f-10bb139\"", None)

            //Usage: Lookup previous ETag and Last-modified, check result for content. If 304 etc get previous response or shortcut parse
            
            printfn "Hello world"
        } |> Async.AwaitTask)
    ]
