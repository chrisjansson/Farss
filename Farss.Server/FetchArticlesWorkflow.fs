module FetchArticlesWorkflow

open Domain
open Persistence
open FeedReaderAdapter
open FSharp.Control.Tasks
open System.Threading.Tasks

type FetchArticlesError =  
    | FeedError of FeedError
    | ItemError of string list

type FetchArticlesForSubscription =  SubscriptionId -> Task<Result<int, FetchArticlesError>>
type FetchArticlesForSubscriptionImpl = SubscriptionRepository -> ArticleRepository -> FeedReaderAdapter -> FetchArticlesForSubscription



let fetchArticlesForSubscriptionImpl: FetchArticlesForSubscriptionImpl = 
    fun subscriptionRepository articleRepository adapter subscriptionId ->
        let getSubscription = subscriptionRepository.get

        let fetchFeedForSubscription subscription = 
            adapter.getFromUrl subscription.Url
            |> Async.map (Result.mapError FetchArticlesError.FeedError)
            |> Async.StartAsTask

        let filterExistingItems (items: Article list) =
            let itemIds = List.map (fun (a: Article) -> a.Guid) items
            let newItemIds = articleRepository.filterExistingArticles subscriptionId itemIds
            List.filter (fun item -> List.contains item.Guid newItemIds) items

        let createArticle = FeedItem.toArticle subscriptionId
        
        let createArticles items = 
            items 
            |> List.map createArticle
            |> Result.traverse
            |> Result.mapError ItemError

        let saveArticle = articleRepository.save
        let saveArticles = List.iter saveArticle

        let aggregateSavedArticles = List.length

        subscriptionId
            |> getSubscription
            |> fetchFeedForSubscription
            |> TaskResult.bind (fun f -> createArticles f.Items)
            |> TaskResult.map filterExistingItems
            |> TaskResult.tee saveArticles
            |> TaskResult.map aggregateSavedArticles

let fetchEntries 
    (subscriptionRepository: SubscriptionRepository) 
    (fetch: FetchArticlesForSubscription)
    _ = 
        //TODO: get all ids or get with projection
        let subscriptions = subscriptionRepository.getAll()

        let executeFetchAsync id = task {
                let! result = Operation.execAsync fetch id
                return (id, result)
            }

        subscriptions
            |> List.map (fun s -> s.Id)
            |> List.map executeFetchAsync
            |> Task.traverse
