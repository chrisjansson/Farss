module Farss.Giraffe

open Giraffe
open Giraffe.EndpointRouting
open Microsoft.AspNetCore.Http

let private requireAuthentication (authenticationScheme: string) = requiresAuthentication (RequestErrors.unauthorized authenticationScheme "Realm" (text "auth failed"))

let echo: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let currentUser = ctx.GetRequestHeader("Remote-User")
        
        text $"%A{currentUser}" next ctx

let endpoints (authenticationScheme: string) =
    [
        GET [
            route "api/ping" (text "pong")
            route "api/echo" echo
            route ApiUrls.GetSubscriptions GetSubscriptionsHandler.getSubscriptionsHandler
            route ApiUrls.GetFileRoute GetFileHandler.getFileHandler
        ]
        POST [
            route ApiUrls.SubscribeToFeed SubscribeToFeedHandler.subscribeToFeedHandler
            route ApiUrls.PreviewSubscribeToFeed SubscribeToFeedHandler.previewSubscribeToFeedHandler
            route ApiUrls.DeleteSubscription DeleteSubscriptionHandler.deleteSubscriptionHandler
            route ApiUrls.PollSubscriptions FetchArticlesHandler.fetchEntriesHandler
            route ApiUrls.GetArticles GetArticlesHandler.getArticlesHandler
            route ApiUrls.SetArticleReadStatus SetArticleReadStatusHandler.setArticleReadStatusHandler
        ]
    ]
    |> Routers.subRoute "farss/"
    |> applyBefore (requireAuthentication authenticationScheme)
    |> List.singleton
