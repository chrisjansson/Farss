module SubscribeToFeedSpecs

open Expecto
open Microsoft.AspNetCore.Mvc.Testing

module HttpClient = 
    let getAsync (url: string) (client: System.Net.Http.HttpClient) =
        client.GetAsync(url) |> Async.AwaitTask

[<Tests>]
let tests = 
    testList "Subscribe to feed specs" [
        testAsync "Subscribe to feed" {
            (*
            setup self hosted http server or subscribe to feed from memory?
                a faked http client with url to xml string mapping should suffice for the time being
            setup Farss server, in memory is okay

            post feed url

            verify against data store
            *)
            let factory = new WebApplicationFactory<Farss.Server.Startup>()
            let client = factory.CreateClient()

            let! response = client |> HttpClient.getAsync "/ping"

            response.EnsureSuccessStatusCode() |> ignore
        }
        
        testAsync "In memory server" {
            let factory = new WebApplicationFactory<Farss.Server.Startup>()
            let client = factory.CreateClient()

            let! response = client |> HttpClient.getAsync "/ping"

            response.EnsureSuccessStatusCode() |> ignore
        }
    ]

