module Farss.Giraffe

open Falco

let createWebAppFalco (): HttpEndpoint list =
    [
        get "/ping" (Response.ofPlainText "pong")
        post ApiUrls.SubscribeToFeed SubscribeToFeedHandler.subscribeToFeedHandler
        post ApiUrls.PreviewSubscribeToFeed SubscribeToFeedHandler.previewSubscribeToFeedHandler
        post ApiUrls.DeleteSubscription DeleteSubscriptionHandler.deleteSubscriptionHandler
        get ApiUrls.GetSubscriptions GetSubscriptionsHandler.getSubscriptionsHandler
        post ApiUrls.PollSubscriptions FetchArticlesHandler.fetchEntriesHandler
        get ApiUrls.GetArticles GetArticlesHandler.getArticlesHandler
        post ApiUrls.SetArticleReadStatus SetArticleReadStatusHandler.setArticleReadStatusHandler
    ]
