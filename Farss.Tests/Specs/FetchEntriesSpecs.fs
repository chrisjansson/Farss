module FetchEntriesSpecs

open Expecto
open Spec
open Persistence
open Domain
open System
open TestStartup

type Entry = unit

open FeedBuilder

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
    Spec.Step.map (fun (_, _) ->
        failwith "Execute real coooode"
    )

let entries entries = 
    Spec.Step.map (fun _ -> entries)


let should_have_been_fetched: AsyncTestStep<Entry list, unit> =
    Spec.Step.map (fun (entries: Entry list, f: TestWebApplicationFactory) -> 
        failwith "Assert"
    )

[<Tests>]
let tests = 
    specs "Fetch feed entries" [
        spec "Fetches entries from feed" <| fun _ ->
            Given >>> a_subscription_for_feed "feed url" >>>
            And >>> feed_has_entries "" [ 
                feedItem "feed title 1" |> toRss 
                feedItem "feed title 2" |> toRss 
            ] >>>
            When >>> feed_is_checked >>>
            Then >>> entries [] >>> should_have_been_fetched
    ]
    