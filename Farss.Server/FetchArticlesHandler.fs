module FetchArticlesHandler

open Giraffe
open Farss.Server.BackgroundTaskQueue
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection

let runFetchArticlesForSubscription
    (serviceProvider: IServiceScopeFactory)
    : FetchArticlesWorkflow.FetchArticlesForSubscription =
    fun id -> Pipeline.runInScopeWithAsync FetchArticlesWorkflow.fetchArticlesForSubscriptionImpl id serviceProvider

let constructFetchEntriesHandler (serviceProvider: IServiceScopeFactory) ct =
    Pipeline.runInScopeWithAsync FetchArticlesWorkflow.queueFetchEntriesForAllSubscriptions ct serviceProvider

let fetchEntriesHandler: HttpHandler =
    fun next (ctx: HttpContext) ->
        let tq = ctx.RequestServices.GetService<IBackgroundTaskQueue>()

        task {
            do! tq.QueuePollArticles(QueueReason.Trigger)
            return! Successful.NO_CONTENT next ctx
        }
