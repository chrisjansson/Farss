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
            
type OperationResult<'T, 'TError> = Result<'T, OperationError<'TError>>
and OperationError<'TError> = 
    | OperationError of exn
    | InnerError of 'TError

let fetchEntries 
    (subscriptionRepository: SubscriptionRepository) 
    (fetch: FetchArticlesForSubscription)
    _ = 
        //get all ids or get with projection
        let subscriptions = subscriptionRepository.getAll()

        //extact to operation module
        let execAsync op arg = task {
                try 
                    let! result = op arg
                    return 
                        match result with
                        | Ok o -> Ok o
                        | Error e -> Error (InnerError e)
                with exn ->
                    return Error (OperationError exn)
            }

        let executeFetchAsync id = task {
                let! result = execAsync fetch id
                return (id, result)
            }

        //extract to task module
        let traverse (tasks: Task<_> list) = 
            let rec inner tasks acc = task {
                    match tasks with
                    | [] -> 
                        return acc
                    | head::tail ->
                        let! r = head
                        let acc = r::acc
                        return! inner tail acc
                }
            inner tasks []
                
        subscriptions
            |> List.map (fun s -> s.Id)
            |> List.map executeFetchAsync
            |> traverse
