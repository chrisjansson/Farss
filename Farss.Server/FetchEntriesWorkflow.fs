module FetchEntriesWorkflow

open Domain
open Persistence
open FeedReaderAdapter
open FSharp.Control.Tasks.V2
open System
open System.Threading.Tasks

type FetchArticlesForSubscription =  SubscriptionId -> Task<Result<int, FeedError>>
type FetchArticlesForSubscriptionImpl = SubscriptionRepository -> ArticleRepository -> FeedReaderAdapter -> FetchArticlesForSubscription

let fetchArticlesForSubscriptionImpl: FetchArticlesForSubscriptionImpl = 
    fun subscriptionRepository articleRepository adapter subscriptionId ->
        let getSubscription = subscriptionRepository.get

        let fetchFeedForSubscription subscription = 
            adapter.getFromUrl subscription.Url |> Async.StartAsTask

        let filterExistingItems feed =
            let itemIds = List.map (fun (fi: Item) -> fi.Id) feed.Items
            let newItemIds = articleRepository.filterExistingArticles subscriptionId itemIds
            List.filter (fun item -> List.contains item.Id newItemIds) feed.Items

        let createArticle item: Article = { 
                Id = Guid.NewGuid(); 
                Guid = item.Id; 
                Subscription = subscriptionId; 
                Title = item.Title; 
                Content = item.Content
                IsRead = false
                PublishedAt = DateTimeOffset.MinValue //todo: parse from feed
            }
        
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
    (fetch: FetchArticlesForSubscription)
    _ = 
        //get all ids or get with projection
        let subscriptions = subscriptionRepository.getAll()

        let executeFetchAsync id = task {
                let! result = Operation.execAsync fetch id
                return (id, result)
            }

        subscriptions
            |> List.map (fun s -> s.Id)
            |> List.map executeFetchAsync
            |> Task.traverse
