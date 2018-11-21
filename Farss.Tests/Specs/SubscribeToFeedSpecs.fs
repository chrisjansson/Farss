module SubscribeToFeedSpecs

open Domain
open Expecto
open TestStartup
open System.Net.Http
open Newtonsoft.Json
open SubscribeToFeedWorkflow
open Spec
open Persistence
open Microsoft.Extensions.DependencyInjection
open System

module HttpClient = 
    let getAsync (url: string) (client: System.Net.Http.HttpClient) =
        client.GetAsync(url) |> Async.AwaitTask

    let postAsync (url: string) (content: 'a) (client: System.Net.Http.HttpClient) =
        let json = JsonConvert.SerializeObject(content)
        let content = new StringContent(json)
        client.PostAsync(url, content) |> Async.AwaitTask


type FeedProjection = { Url: string }

let project (feed: Domain.Subscription): FeedProjection =
    { Url = feed.Url }

let feed_available_at_url (url: string) (feed: string): AsyncTestStep<_, unit> =
    fun atc -> async {
        let! (_, f) = atc
            
        f.FakeFeedReader.Add (url, feed)

        return ((), f)
    }

let a_user_subscribes_to_feed (url: string): AsyncTestStep<_, unit> =
    fun atc -> async {
        let! (_, f) = atc
            
        let payload: SubscribeToFeedCommand = { Url = url }
        let client = f.CreateClient()
        let! response = client |> HttpClient.postAsync "/feeds" payload
        response.EnsureSuccessStatusCode() |> ignore

        return ((), f)
    }

let default_feed_with_url (url: string): AsyncTestStep<_, _> =
    fun atc -> async {
        let! (_, f) = atc
        return ({ FeedProjection.Url = url }, f)
    }

let should_have_been_saved: AsyncTestStep<FeedProjection, _> =
    fun atc -> async {
        let! (expected, f)  = atc
        let actualFeeds = (inScope (fun scope -> 
            let fr = scope.GetService<SubscriptionRepository>()
            fr.getAll()) f) |> List.map project

        Expect.equal actualFeeds [ expected ] "one added feed"
        return ((), f)
    }

let a_feed_with_url (url: string): AsyncTestStep<_, unit> =
    fun atc -> async {
        let! (_, f) = atc

        inScope (fun s -> 
            let r = s.GetService<SubscriptionRepository>()
            let feed = { Domain.Url = url; Id = Guid.NewGuid() }
            r.save feed
        ) f

        return  ((), f)
    }

let subscriptions_are_fetched: AsyncTestStep<_, string> =
    fun atc -> async {
        let! (_, f) = atc

        let client = f.CreateClient()
        let! response = client |> HttpClient.getAsync "/feeds"
        response.EnsureSuccessStatusCode() |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return (content, f)
    }

let subscription_with_url (expectedUrl: string): AsyncTestStep<string, string * string> = 
    fun atc -> async {
        let! (content, f) = atc

        return ((expectedUrl, content), f)
    }

    
let is_returned: AsyncTestStep<string*string, _> =
    fun atc -> async {
        let! ((expectedUrl, actualContent),f) = atc

        let dto = JsonConvert.DeserializeObject<Dto.SubscriptionDto[]>(actualContent)
    
        Expect.equal dto.Length 1 "Number of feeds returned"
        Expect.all dto (fun s -> s.Url = expectedUrl) "feed subscription url"
        return ((), f)
    }

let specCaseAsync name t =
    testAsync name {
        let wrapper: unit -> AsyncTestStep<unit, _> =
            fun _ atc -> async {
                let! (_, f) = atc
                let! result = t f
                return (result, f)
            }
        do! wrapper |> toTest
    } 

let feed_with_url (url: string): AsyncTestStep<_, string> =
    fun atc -> async {
        let! (_, f) = atc
        return (url, f)
    }
    
let is_deleted: AsyncTestStep<string, _> =
    fun atc -> async {
        let! (url, f) = atc

        let subscription = (inScope (fun s -> 
            let r = s.GetService<SubscriptionRepository>()
            r.getAll() |> List.find (fun s -> s.Url = url)
        ) f)

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
        
        let subscriptions = 
            inScope (fun s -> 
                let r = s.GetService<SubscriptionRepository>()
                r.getAll()
            ) f
        
        Expect.equal subscriptions.Length 1 "Only one subscription should remain"
        Expect.all subscriptions (fun s -> s.Url = expectedRemainingUrl) "Subscription with url should remain"

        return ((), f)
    }

[<Tests>]
let tests = 
    specs "Subscribe to feed specs" [
        spec "Subscribe to feed" <| fun _->
            let feedContent = FeedBuilder.feedItem "feed title" |> FeedBuilder.toRss            

            Given >>> feed_available_at_url "a feed url" feedContent >>>
            When >>> a_user_subscribes_to_feed "a feed url" >>
            Then >>> default_feed_with_url "a feed url" >>> should_have_been_saved
            
        spec "Get subscriptions" <| fun _ ->
            Given >>> a_feed_with_url "http://whatevs" >>> 
            When >>> subscriptions_are_fetched >>> 
            Then >>> subscription_with_url "http://whatevs" >>> is_returned
        
        spec "Delete subscription" <| fun _ ->
            Given >>> a_feed_with_url "feed 1" >>> 
            And >>> a_feed_with_url "feed 2" >>> 
            When >>> feed_with_url "feed 2" >>> is_deleted >>>
            Then >>> only_feed_with_url "feed 1" >>> should_remain
    ]

