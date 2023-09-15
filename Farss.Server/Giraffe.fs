module Farss.Giraffe

open Giraffe
open Giraffe.EndpointRouting

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
