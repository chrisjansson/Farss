module FetchArticlesHandler

open Giraffe
open Farss.Server.BackgroundTaskQueue
open Persistence
open FeedReaderAdapter
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open System

let runFetchArticlesForSubscription
    (serviceProvider: IServiceScopeFactory)
    : FetchArticlesWorkflow.FetchArticlesForSubscription =
    fun id -> Pipeline.runInScopeWithAsync FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl id serviceProvider


let constructFetchEntriesHandler (serviceProvider: IServiceScopeFactory) ct =
    Pipeline.runInScopeWithAsync FetchArticlesWorkflow.queueFetchEntriesForAllSubscriptions ct serviceProvider

let private runUpdateIconForSubscription (serviceProvider: IServiceProvider) =
    fun id ->
        task {
            //todo: uow
            use scope = serviceProvider.CreateScope()
            let services = scope.ServiceProvider
            let adapter = services.GetService<FeedReaderAdapter>()
            let subscriptionRepository = services.GetService<SubscriptionRepository>()
            let fileRepository = services.GetService<FileRepository>()
            return! SubscribeToFeedWorkflow.updateFeedIcon adapter subscriptionRepository fileRepository id
        }

let constructUpdateIconsHandler (serviceProvider: IServiceScopeFactory) =

    let run () =
        task {
            use scope = serviceProvider.CreateScope()
            let services = scope.ServiceProvider
            let subscriptionRepository = services.GetService<SubscriptionRepository>()
            let subs = subscriptionRepository.getAll () |> List.map (fun x -> x.Id)

            for s in subs do
                let! _ = runUpdateIconForSubscription services s
                ()

            return ()
        }

    run


let fetchEntriesHandler: HttpHandler =
    fun next (ctx: HttpContext) ->
        let tq = ctx.RequestServices.GetService<IBackgroundTaskQueue>()

        task {
            do! tq.QueuePollArticles(QueueReason.Trigger)

            return! Successful.NO_CONTENT next ctx
        }
