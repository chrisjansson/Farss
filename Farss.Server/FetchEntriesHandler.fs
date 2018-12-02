module FetchEntriesHandler

open Persistence
open FeedReaderAdapter
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Extensions.DependencyInjection

let fetchEntriesHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let runFetchArticlesForSubscription: FetchEntriesWorkflow.FetchArticlesForSubscription = 
                fun id -> task {
                    //todo: uow
                    use scope = ctx.RequestServices.CreateScope()
                    let services = scope.ServiceProvider
                    let adapter = services.GetService<FeedReaderAdapter>()
                    let subscriptionRepository = services.GetService<SubscriptionRepository>()
                    let articleRepository = services.GetService<ArticleRepository>()
                    return! FetchEntriesWorkflow.fetchArticlesForSubscriptionImpl subscriptionRepository articleRepository adapter id
                }
            
            let subscriptionRepository = ctx.GetService<SubscriptionRepository>()

            let! result = FetchEntriesWorkflow.fetchEntries subscriptionRepository runFetchArticlesForSubscription ()
            //todo: do something smart, like log the result or whatever
            result |> ignore

            return! Successful.NO_CONTENT next ctx
        }        
       