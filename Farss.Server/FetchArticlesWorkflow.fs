﻿module FetchArticlesWorkflow

open System
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
    BackendSubscriptionRepository * BackendArticleRepository * FeedReaderAdapter -> FetchArticlesForSubscription

let fetchArticlesForSubscriptionImpl: FetchArticlesForSubscriptionImpl =
    fun (subscriptionRepository, articleRepository, adapter) subscriptionId ->
        let getSubscription = subscriptionRepository.get

        let fetchFeedForSubscription (subscription: TenantedSubscription) =
            adapter.getFromUrl subscription.Url
            |> Task.map (Result.mapError FetchArticlesError.FeedError)

        let filterExistingItems (items: TenantedArticle list) =
            let itemIds = List.map (fun (a: TenantedArticle) -> a.Guid) items
            let newItemIds = articleRepository.filterExistingArticles subscriptionId itemIds
            List.filter (fun item -> List.contains item.Guid newItemIds) items

        let createArticle tenantId = FeedItem.toArticle subscriptionId tenantId

        let createArticles tenantId items =
            items |> List.map (createArticle tenantId) |> Result.traverse |> Result.mapError ItemError

        let saveArticle = articleRepository.save
        let saveArticles = List.iter saveArticle

        let aggregateSavedArticles = List.length

        let logFetch tenantId r =
            let message = 
                match r with
                | Ok savedArticles -> Ok $"Fetched {savedArticles} new article(s)"
                | Error e -> Error $"%A{e}"
            
            subscriptionRepository.storeLog (subscriptionId, tenantId, message, DateTimeOffset.UtcNow)
            
        let subscription = getSubscription subscriptionId
            
        subscription
        |> fetchFeedForSubscription
        |> TaskResult.bind (fun f -> createArticles subscription.TenantId f.Items)
        |> TaskResult.map filterExistingItems
        |> TaskResult.tee saveArticles
        |> TaskResult.map aggregateSavedArticles
        |> Task.tee (logFetch subscription.TenantId)

let queueFetchEntriesForAllSubscriptions
    (subscriptionRepository: BackendSubscriptionRepository, queue: IBackgroundTaskQueue)
    (ct: CancellationToken)
    =
    task {
        let subscriptions = subscriptionRepository.getAll ()

        for s in subscriptions do
            do! queue.QueuePollArticlesForSubscription(s.Id, ct)
    }
