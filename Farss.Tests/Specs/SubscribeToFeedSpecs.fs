module SubscribeToFeedSpecs

open Domain
open Expecto
open TestStartup
open Persistence
open System.IO
open System.Net.Http
open Newtonsoft.Json
open SubscribeToFeedWorkflow

module HttpClient = 
    let getAsync (url: string) (client: System.Net.Http.HttpClient) =
        client.GetAsync(url) |> Async.AwaitTask

    let postAsync (url: string) (content: 'a) (client: System.Net.Http.HttpClient) =
        let json = JsonConvert.SerializeObject(content)
        let content = new StringContent(json)
        client.PostAsync(url, content) |> Async.AwaitTask

let ``then`` (f: TestWebApplicationFactory) = f

let ``feed`` (url: string) cont (f: Async<TestWebApplicationFactory>) = async {
        let! f' = f
        do! cont ({ Feed.Url = url }, f')
    }

let ``should have been saved`` (op: Feed * TestWebApplicationFactory) = async {
        let (f, f') = op
        let repository = f'.Server.Host.Services.GetService(typeof<FeedRepository>) :?> FeedRepository
        let actualFeeds = repository.getAll()
        Expect.equal actualFeeds [ f ] "one added feed"
    }

let Given () = 
    new TestWebApplicationFactory()

let ``feed available at url`` (url: string) (feed: string) (factory: TestWebApplicationFactory) = 
    factory.FakeFeedReader.Add (url, feed)
    factory

let ``a user subscribes to feed`` (url: string) (factory: TestWebApplicationFactory) = async {
        let payload: SubscribeToFeedCommand = { Url = url }
        let client = factory.CreateClient()
        let! response = client |> HttpClient.postAsync "/feeds" payload
        response.EnsureSuccessStatusCode() |> ignore
        return factory
    }

let When a = a
let Then a = a


[<Tests>]
let tests = 
    testList "Subscribe to feed specs" [
        testAsync "Subscribe to feed" {
            let feedContent = File.ReadAllText("ExampleRssFeed.xml")
            do!
                Given () |> ``feed available at url`` "a feed url" feedContent |> 
                When |> ``a user subscribes to feed`` "a feed url" |>
                Then |> ``feed`` "a feed url" ``should have been saved``
        }
        
        ptest "Get subscription" {
            ()
        }
        
        ptest "Delete subscription" {
            ()
        }
        
        testAsync "In memory server" {
            let factory = new TestWebApplicationFactory()
            let client = factory.CreateClient()

            let! response = client |> HttpClient.getAsync "/ping"

            response.EnsureSuccessStatusCode() |> ignore
        }
    ]

