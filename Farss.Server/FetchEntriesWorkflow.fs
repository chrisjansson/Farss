module FetchEntriesWorkflow

open Domain
open Persistence
open FeedReaderAdapter
open FSharp.Control.Tasks.V2
open System
open System.Threading.Tasks


let fetchEntries 
    (subscriptionRepository: SubscriptionRepository) 
    (articleRepository: ArticleRepository) 
    (adapter: FeedReaderAdapter) 
    _ = task {
        let subscriptions = subscriptionRepository.getAll()
    
        let tasks = 
            subscriptions 
            |> List.map (fun s -> adapter.getFromUrl s.Url)
            |> List.map Async.StartAsTask
            |> Array.ofList
        let! results = Task.WhenAll(tasks)

        let extractError r =
            match r with
            | Error e -> [e]
            | _ -> []

        let errors =
            results 
            |> List.ofArray
            |> List.collect extractError

        //for s in subscriptions do
        //    let! getFeedResult = adapter.getFromUrl(s.Url)
        //    match getFeedResult with
        //    | Ok feed ->
        //        for item in feed.Items do
        //            let article = { Title = item.Title; Id = Guid.NewGuid() }
        //            articleRepository.save(article)
        //    | Error e -> ()

        return Ok errors
    }
