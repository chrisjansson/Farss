module FetchArticlesHandler

open Giraffe
open Farss.Server.BackgroundTaskQueue
open Persistence
open FeedReaderAdapter
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open System

let runFetchArticlesForSubscription
    (serviceProvider: IServiceProvider)
    : FetchArticlesWorkflow.FetchArticlesForSubscription =
    fun id ->
        task {
            //todo: uow
            use scope = serviceProvider.CreateScope()
            let services = scope.ServiceProvider
            let adapter = services.GetService<FeedReaderAdapter>()
            let subscriptionRepository = services.GetService<SubscriptionRepository>()
            let articleRepository = services.GetService<ArticleRepository>()

            return!
                FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl
                    subscriptionRepository
                    articleRepository
                    adapter
                    id
        }

let constructFetchEntriesHandler (serviceProvider: IServiceProvider) ct =
    let subscriptionRepository = serviceProvider.GetService<SubscriptionRepository>()
    let queue = serviceProvider.GetService<IBackgroundTaskQueue>()
    FetchArticlesWorkflow.queueFetchEntriesForAllSubscriptions subscriptionRepository queue ct

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

let constructUpdateIconsHandler (serviceProvider: IServiceProvider) =

    let run () =
        task {
            let services = serviceProvider
            let subscriptionRepository = services.GetService<SubscriptionRepository>()
            let subs = subscriptionRepository.getAll () |> List.map (fun x -> x.Id)

            for s in subs do
                let! _ = runUpdateIconForSubscription serviceProvider s
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
