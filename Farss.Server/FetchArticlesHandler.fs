module FetchArticlesHandler

open Giraffe
open Farss.Server.BackgroundTaskQueue
open Persistence
open FeedReaderAdapter
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open System

let constructFetchEntriesHandler (serviceProvider: IServiceProvider) =
    let runFetchArticlesForSubscription: FetchArticlesWorkflow.FetchArticlesForSubscription = 
        fun id -> task {
            //todo: uow
            use scope = serviceProvider.CreateScope()
            let services = scope.ServiceProvider
            let adapter = services.GetService<FeedReaderAdapter>()
            let subscriptionRepository = services.GetService<SubscriptionRepository>()
            let articleRepository = services.GetService<ArticleRepository>()
            return! FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl subscriptionRepository articleRepository adapter id
        }
            
    let subscriptionRepository = serviceProvider.GetService<SubscriptionRepository>()

    FetchArticlesWorkflow.fetchEntries subscriptionRepository runFetchArticlesForSubscription

let fetchEntriesHandler: HttpHandler =
    fun next (ctx: HttpContext) ->
        let tq = ctx.RequestServices.GetService<IBackgroundTaskQueue>()
        task {
            do! tq.QueuePollArticles(QueueReason.Trigger)

            return! Successful.NO_CONTENT next ctx
        }