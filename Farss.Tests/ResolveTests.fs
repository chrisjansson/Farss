module Farss.Tests.ResolveTests

open Expecto
open Farss.Server
open Microsoft.Extensions.DependencyInjection

[<Tests>]
let ResolveTests =
    testList "Resolve services" [
        testCase "Resolves single service"
        <| fun _ ->
            let sc = ServiceCollection()
            sc.AddTransient<string>(fun _ -> "Hello") |> ignore
            let sp = sc.BuildServiceProvider()

            let testFunc (a: string) () = a

            let actual = Resolve.resolve sp testFunc

            let result = actual ()
            Expect.equal result "Hello" "Resolved service"

        testCase "Resolves multiple services"
        <| fun _ ->
            let sc = ServiceCollection()
            sc.AddTransient<string>(fun _ -> "Hello") |> ignore
            sc.AddTransient<obj>(fun _ -> ()) |> ignore
            let sp = sc.BuildServiceProvider()

            let testFunc (a: string, b: unit) () = (a, b)

            let actual = Resolve.resolve sp testFunc

            let result = actual ()
            Expect.equal result ("Hello", ()) "Resolved service"
    ]
