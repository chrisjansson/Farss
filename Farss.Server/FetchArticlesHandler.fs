module FetchArticlesHandler

open System.Net
open Falco.Core
open Farss.Server.BackgroundTaskQueue
open Persistence
open FeedReaderAdapter
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
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
    fun (ctx: HttpContext) ->
        let tq = ctx.RequestServices.GetService<IBackgroundTaskQueue>()
        task {
            do! tq.QueuePollArticles(QueueReason.Trigger)
            let successNoContent ctx =
                ctx
                |> Falco.Response.withStatusCode (int HttpStatusCode.NoContent)
                |> Falco.Response.ofEmpty
            
            return successNoContent ctx
        }