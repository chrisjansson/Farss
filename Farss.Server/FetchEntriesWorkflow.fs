module FetchEntriesWorkflow

open Domain
open Persistence
open FeedReaderAdapter
open FSharp.Control.Tasks.V2
open System
open System.Threading.Tasks

module Result = 
    let tee (f: 'T -> unit) (r: Result<'T, _>)  =
        match r with
        | Ok o -> 
            f o
            r
        | _ -> r
    let teeError (f: 'TError -> unit) (r: Result<_, 'TError>) =
        match r with
        | Error e -> 
            f e
            r
        | _ -> r


type FetchEntriesForSubscription =  SubscriptionId -> Task<Result<int, FeedError>>
type FetchEntriesForSubscriptionImpl = SubscriptionRepository -> ArticleRepository -> FeedReaderAdapter -> FetchEntriesForSubscription

let fetchEntriesForSubscriptionImpl: FetchEntriesForSubscriptionImpl = 
    fun subscriptionRepository articleRepository adapter subscriptionId ->
        let getSubscription subscriptionId =
            //Todo: elevate to repository
            subscriptionRepository.getAll()
            |> List.find (fun s -> s.Id = subscriptionId)

        let fetchFeedForSubscription subscription = 
            adapter.getFromUrl subscription.Url |> Async.StartAsTask

        let filterExistingItems feed =
            //Todo: elevate to repository
            let existingArticles = articleRepository.getAll()
            let isNewArticle (item: Item) = 
                let hasArticle = List.exists (fun (a: Article) -> a.Guid = item.Id) existingArticles
                not hasArticle

            feed.Items
            |> List.filter isNewArticle
        
        let createArticle item: Article =
            { Title = item.Title; Id = Guid.NewGuid(); Guid = item.Id }
        
        let createArticles = List.map createArticle

        let saveArticle = articleRepository.save
        let saveArticles = List.iter saveArticle

        let aggregateSavedArticles = List.length

        subscriptionId
            |> getSubscription
            |> fetchFeedForSubscription
            |> TaskResult.map filterExistingItems
            |> TaskResult.map createArticles
            |> TaskResult.tee saveArticles
            |> TaskResult.map aggregateSavedArticles

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

        let extractOk r =
            match r with
            | Ok r -> [r]
            | _ -> []

        let feeds = 
            results 
            |> List.ofArray
            |> List.collect extractOk

        for feed in feeds do
            for item in feed.Items do
                let hasArticle (guid: string) =
                    articleRepository.getAll()
                    |> List.exists (fun a -> a.Guid = guid)
                if not (hasArticle item.Id) then do
                    let article = { Article.Title = item.Title; Id = Guid.NewGuid(); Guid = item.Id }
                    articleRepository.save(article)
        
        let errors =
            results 
            |> List.ofArray
            |> List.collect extractError

        return Ok errors
    }
