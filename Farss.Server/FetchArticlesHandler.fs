module FetchArticlesHandler

open Persistence
open FeedReaderAdapter
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Extensions.DependencyInjection
open System

let constructFetchEntriesHandler (serviceProvider: IServiceProvider) =
    let runFetchArticlesForSubscription: FetchEntriesWorkflow.FetchArticlesForSubscription = 
        fun id -> task {
            //todo: uow
            use scope = serviceProvider.CreateScope()
            let services = scope.ServiceProvider
            let adapter = services.GetService<FeedReaderAdapter>()
            let subscriptionRepository = services.GetService<SubscriptionRepository>()
            let articleRepository = services.GetService<ArticleRepository>()
            return! FetchEntriesWorkflow.fetchArticlesForSubscriptionImpl subscriptionRepository articleRepository adapter id
        }
            
    let subscriptionRepository = serviceProvider.GetService<SubscriptionRepository>()

    FetchEntriesWorkflow.fetchEntries subscriptionRepository runFetchArticlesForSubscription

let fetchEntriesHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let workflow = constructFetchEntriesHandler ctx.RequestServices
            let! result = workflow ()

            //todo: do something smart, like log the result or whatever
            result |> ignore

            return! Successful.NO_CONTENT next ctx
        }        
       