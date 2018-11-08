module SubscribeToFeedSpecs

open Domain
open Expecto
open TestStartup
open Persistence
open System.Net.Http
open Newtonsoft.Json
open SubscribeToFeedWorkflow
open System

module HttpClient = 
    let getAsync (url: string) (client: System.Net.Http.HttpClient) =
        client.GetAsync(url) |> Async.AwaitTask

    let postAsync (url: string) (content: 'a) (client: System.Net.Http.HttpClient) =
        let json = JsonConvert.SerializeObject(content)
        let content = new StringContent(json)
        client.PostAsync(url, content) |> Async.AwaitTask

type TC<'C> = 'C * TestWebApplicationFactory
type ATC<'C> = Async<TC<'C>>

let ``then`` (f: TestWebApplicationFactory) = f

type FeedProjection = { Url: string }

let project (feed: Domain.Feed): FeedProjection =
    { Url = feed.Url }

let ``default feed with url`` (url: string) cont (f: Async<TestWebApplicationFactory>) = async {
        let! f' = f
        do! cont ({ FeedProjection.Url = url }, f')
    }

let ``should have been saved`` (op: FeedProjection * TestWebApplicationFactory) = async {
        let (f, f') = op
        let repository = f'.Server.Host.Services.GetService(typeof<FeedRepository>) :?> FeedRepository
        let actualFeeds = repository.getAll() |> List.map project
        Expect.equal actualFeeds [ f ] "one added feed"
    }

let Given () = 
    let f = new TestWebApplicationFactory()
    f.CreateClient() |> ignore
    f

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

let ``a feed with url`` (url: string) (f: TestWebApplicationFactory) =
    let repository = f.Server.Host.Services.GetService(typeof<FeedRepository>) :?> FeedRepository
    let feed = { Domain.Url = url; Id = Guid.NewGuid() }
    repository.save feed
    f

let ``subscriptions are fetched`` (f: TestWebApplicationFactory) = async {
        let client = f.CreateClient()
        let! response = client |> HttpClient.getAsync "/feeds"
        response.EnsureSuccessStatusCode() |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return (content, f)
    }
    
let ``subscription with url`` (expectedUrl: string) cont (tc: ATC<_>) = async {
        let! (content, _) = tc
        cont (expectedUrl, content)
    }

let ``is returned`` (expectedUrl: string, actualContent: string) =
    let dto = JsonConvert.DeserializeObject<Dto.SubscriptionDto[]>(actualContent)
    
    Expect.equal dto.Length 1 "Number of feeds returned"
    Expect.all dto (fun s -> s.Url = expectedUrl) "feed subscription url"

[<Tests>]
let tests = 
    testList "Subscribe to feed specs" [
        testAsync "Subscribe to feed" {
            let feedContent = FeedBuilder.feed "feed title" |> FeedBuilder.toRss            

            do!
                Given () |> ``feed available at url`` "a feed url" feedContent |> 
                When |> ``a user subscribes to feed`` "a feed url" |>
                Then |> ``default feed with url`` "a feed url" ``should have been saved``
        }
        
        testAsync "Get subscription" {
            do! 
                Given () |> ``a feed with url`` "http://whatevs" |>
                When |> ``subscriptions are fetched`` |>
                Then |> ``subscription with url`` "http://whatevs" ``is returned``
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

