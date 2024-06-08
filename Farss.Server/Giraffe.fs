module Farss.Giraffe

open Giraffe
open Giraffe.EndpointRouting

let requireAuthentication = requiresAuthentication (RequestErrors.unauthorized "DefaultScheme" "Realm" (text "auth failed"))

let endpoints  =
    [
        GET [
            route "/ping" (text "pong")
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
    |> Routers.subRoute "farss/api"
    |> applyBefore requireAuthentication
    |> List.singleton
