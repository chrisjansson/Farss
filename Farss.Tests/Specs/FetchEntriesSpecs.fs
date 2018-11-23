module FetchEntriesSpecs

open Expecto
open Spec
open Persistence
open Domain
open System
open TestStartup

open FeedBuilder
open SubscribeToFeedSpecs

type ExpectedArticle = 
    {
        Title: string
    }

let a_subscription_for_feed (url: string) =
    Spec.Step.map (fun (_, f) ->
            withService<SubscriptionRepository> (fun r -> 
                let a = { Url = url; Id = Guid.NewGuid() }
                r.save a
            ) f
        )

let feed_has_entries url feeds = 
    Spec.Step.map (fun (_, f) -> 
        for feed in feeds do
            f.FakeFeedReader.Add(url, feed)
    )
        
let feed_is_checked: AsyncTestStep<_, unit> =
    Spec.Step.mapAsync (fun (_, f) -> async {
        let client = f.CreateClient()
        do! HttpClient.postAsync "/poll" () client |> Async.Ignore
    })

let articles entries = 
    Spec.Step.map (fun _ -> entries)


let should_have_been_fetched: AsyncTestStep<ExpectedArticle list, unit> =
    Spec.Step.map (fun (expected: ExpectedArticle list, f: TestWebApplicationFactory) -> 
        let articles: Article list = f.InScope(fun r -> r.getAll())

        let actualArticles = 
            articles 
            |> List.map (fun a -> { ExpectedArticle.Title = a.Title })

        Expect.equal actualArticles expected "articles"
    )

let article title: ExpectedArticle =
    {
        Title = title
    }

[<Tests>]
let tests = 
    specs "Fetch feed entries" [
        spec "Fetches entries from feed" <| fun _ ->
            Given >>> a_subscription_for_feed "feed url" >>>
            And >>> feed_has_entries "feed url" [ 
                feedItem "article title 1" |> toRss 
                feedItem "article title 2" |> toRss 
            ] >>>
            When >>> feed_is_checked >>>
            Then >>> articles [
                article "article title 1"
                article "article title 2"
            ] >>> should_have_been_fetched
    ]
    