module SubscribeToFeedSpecs

open Domain
open Expecto
open Microsoft.AspNetCore.Mvc.Testing
open TestStartup
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.AspNetCore.Mvc.Testing
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

[<Tests>]
let tests = 
    testList "Subscribe to feed specs" [
        testAsync "Subscribe to feed" {
            let factory = new TestWebApplicationFactory()

            let feedContent = File.ReadAllText("ExampleRssFeed.xml")
            factory.FakeFeedReader.Add ("a feed url", feedContent)

            let client = factory.CreateClient()

            let payload: SubscribeToFeedCommand = { Url = "a feed url" }
            let! response = client |> HttpClient.postAsync "/feeds" payload
            response.EnsureSuccessStatusCode() |> ignore

            let repository = factory.Server.Host.Services.GetService(typeof<FeedRepository>) :?> FeedRepository
            
            let expected: Feed = 
                {
                    Url = "a feed url"
                }
                
            let actualFeeds = repository.getAll()
            Expect.equal actualFeeds [ expected ] "one added feed"
        }
        
        testAsync "In memory server" {
            let factory = new TestWebApplicationFactory()
            let client = factory.CreateClient()

            let! response = client |> HttpClient.getAsync "/ping"

            response.EnsureSuccessStatusCode() |> ignore
        }
    ]

