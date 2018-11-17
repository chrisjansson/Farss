module SubscribeToFeedSpecs

open Domain
open Expecto
open TestStartup
open Persistence
open System.Net.Http
open Newtonsoft.Json
open SubscribeToFeedWorkflow
open System
open Microsoft.Extensions.DependencyInjection

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

let inScope op (f: TestWebApplicationFactory) =
    use scope = f.Server.Host.Services.CreateScope()
    op scope.ServiceProvider

let ``should have been saved`` (op: FeedProjection * TestWebApplicationFactory) = async {
        let (f, f') = op

        let actualFeeds = (inScope (fun scope -> 
            let fr = scope.GetService<FeedRepository>()
            fr.getAll()) f') |> List.map project

        Expect.equal actualFeeds [ f ] "one added feed"
    }

let Given () = 
    let df = DatabaseTesting.createFixture2 ()
    let f = new TestWebApplicationFactory(df)
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

type AsyncTestStep<'T, 'U> = ATC<'T> -> ATC<'U>

let (>>>) (l: AsyncTestStep<'a,'b>) (r: AsyncTestStep<'b,'c>): AsyncTestStep<'a, 'c> = 
    let f arg = 
        let nextAtc = l arg
        r nextAtc
    f

let pipe: AsyncTestStep<_, _> =
        fun atc -> async {
        let! (x, f) = atc
        return  (x, f)
    }

let Given2 = pipe
let When2 = pipe
let Then2 = pipe
let And = pipe

let ``a feed with url2`` (url: string): AsyncTestStep<_, unit> =
    fun atc -> async {
        let! (_, f) = atc

        let repository = f.Server.Host.Services.GetService(typeof<FeedRepository>) :?> FeedRepository
        let feed = { Domain.Url = url; Id = Guid.NewGuid() }
        repository.save feed

        return  ((), f)
    }

let ``subscriptions are fetched2``: AsyncTestStep<_, string> =
    fun atc -> async {
        let! (_, f) = atc

        let client = f.CreateClient()
        let! response = client |> HttpClient.getAsync "/feeds"
        response.EnsureSuccessStatusCode() |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return (content, f)
    }

let ``subscription with url2`` (expectedUrl: string): AsyncTestStep<string, string * string> = 
    fun atc -> async {
        let! (content, f) = atc

        return ((expectedUrl, content), f)
    }

    
let ``is returned2``: AsyncTestStep<string*string, _> =
    fun atc -> async {
        let! ((expectedUrl, actualContent),f) = atc

        let dto = JsonConvert.DeserializeObject<Dto.SubscriptionDto[]>(actualContent)
    
        Expect.equal dto.Length 1 "Number of feeds returned"
        Expect.all dto (fun s -> s.Url = expectedUrl) "feed subscription url"
        return ((), f)
    }
  
let toTest (testStep: AsyncTestStep<unit, _>) = async {
        let df = DatabaseTesting.createFixture2 ()
        let f = new TestWebApplicationFactory(df)
        f.CreateClient() |> ignore

        let stuff: ATC<unit> = async.Return ((), f)
        do! testStep stuff |> Async.Ignore
    }
    
let testF name t = 
    testAsync name {
        do! t |> toTest
    }

let feed_with_url (url: string): AsyncTestStep<_, string> =
    fun atc -> async {
        let! (_, f) = atc
        return (url, f)
    }
    
let is_deleted: AsyncTestStep<string, _> =
    fun atc -> async {
        let! (url, f) = atc

        let repository = f.Server.Host.Services.GetService(typeof<FeedRepository>) :?> FeedRepository
        let subscription = 
            repository.getAll()
            |> List.find (fun s -> s.Url = url)

        let command: Domain.DeleteSubscriptionCommand = { Id = subscription.Id }

        let! response = f.CreateClient() |> HttpClient.postAsync "/subscription/delete" command
        response.EnsureSuccessStatusCode() |> ignore

        return ((), f)
    }

let only_feed_with_url (url: string): AsyncTestStep<_, string> =
    fun atc -> async {
        let! (_, f) = atc
        return (url, f)
    }

let should_remain: AsyncTestStep<string, _> =
    fun atc -> async {
        let! (expectedRemainingUrl, f) = atc
        
        let repository = f.Server.Host.Services.GetService(typeof<FeedRepository>) :?> FeedRepository
        let subscriptions = repository.getAll()
        
        Expect.equal subscriptions.Length 1 "Only one subscription should remain"
        Expect.all subscriptions (fun s -> s.Url = expectedRemainingUrl) "Subscription with url should remain"

        return ((), f)
    }

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
            
        testF "Get subscriptions" (
            Given2 >>> ``a feed with url2`` "http://whatevs" >>> 
            When2 >>> ``subscriptions are fetched2`` >>> 
            Then2 >>> ``subscription with url2`` "http://whatevs" >>> ``is returned2``
        )
        
        testF "Delete subscription" (
            Given2 >>> ``a feed with url2`` "feed 1" >>> 
            And >>> ``a feed with url2`` "feed 2" >>> 
            When2 >>> feed_with_url "feed 2" >>> is_deleted >>>
            Then2 >>> only_feed_with_url "feed 1" >>> should_remain
        )

        testAsync "In memory server" {
            let df = DatabaseTesting.createFixture2 ()
            let factory = new TestWebApplicationFactory(df)
            let client = factory.CreateClient()

            let! response = client |> HttpClient.getAsync "/ping"

            response.EnsureSuccessStatusCode() |> ignore
        }
    ] |> testSequencedGroup "integration"

