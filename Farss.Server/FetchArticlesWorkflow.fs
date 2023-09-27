module FetchArticlesWorkflow

open System.Threading
open Domain
open Farss.Server.BackgroundTaskQueue
open Persistence
open FeedReaderAdapter
open System.Threading.Tasks

type FetchArticlesError =
    | FeedError of FeedError
    | ItemError of string list

type FetchArticlesForSubscription = SubscriptionId -> Task<Result<int, FetchArticlesError>>

type FetchArticlesForSubscriptionImpl =
    SubscriptionRepository * ArticleRepository * FeedReaderAdapter -> FetchArticlesForSubscription

let fetchArticlesForSubscriptionImpl: FetchArticlesForSubscriptionImpl =
    fun (subscriptionRepository, articleRepository, adapter) subscriptionId ->
        let getSubscription = subscriptionRepository.get

        let fetchFeedForSubscription (subscription: Subscription) =
            adapter.getFromUrl subscription.Url
            |> Async.map (Result.mapError FetchArticlesError.FeedError)
            |> Async.StartAsTask

        let filterExistingItems (items: Article list) =
            let itemIds = List.map (fun (a: Article) -> a.Guid) items
            let newItemIds = articleRepository.filterExistingArticles subscriptionId itemIds
            List.filter (fun item -> List.contains item.Guid newItemIds) items

        let createArticle = FeedItem.toArticle subscriptionId

        let createArticles items =
            items |> List.map createArticle |> Result.traverse |> Result.mapError ItemError

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

let queueFetchEntriesForAllSubscriptions
    (subscriptionRepository: SubscriptionRepository, queue: IBackgroundTaskQueue)
    (ct: CancellationToken)
    =
    task {
        let subscriptions = subscriptionRepository.getAll ()

        for s in subscriptions do
            do! queue.QueuePollArticlesForSubscription(s.Id, ct)
    }
