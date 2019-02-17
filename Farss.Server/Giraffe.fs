module Farss.Giraffe

open Giraffe

let createWebApp () =
    choose [
        route "/ping"   >=> text "pong"
        route "/"       >=> htmlFile "/pages/index.html"
        route ApiUrls.SubscribeToFeed >=> POST >=> SubscribeToFeedHandler.subscribeToFeedHandler
        route ApiUrls.PreviewSubscribeToFeed >=> POST >=> SubscribeToFeedHandler.previewSubscribeToFeedHandler
        route ApiUrls.GetSubscriptions >=> GET >=> GetSubscriptionsHandler.getSubscriptionsHandler
        route ApiUrls.DeleteSubscription >=> POST >=> DeleteSubscriptionHandler.deleteSubscriptionHandler
        route ApiUrls.PollSubscriptions >=> POST >=> FetchArticlesHandler.fetchEntriesHandler
        route ApiUrls.GetArticles >=> GET >=> GetArticlesHandler.getArticlesHandler
        route ApiUrls.SetArticleReadStatus >=> POST >=> SetArticleReadStatusHandler.setArticleReadStatusHandler
    ]
