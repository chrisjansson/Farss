module Farss.Giraffe

open Giraffe

let createWebApp () =
    choose [
        route "/ping"   >=> text "pong"
        route "/"       >=> htmlFile "/pages/index.html"
        route "/feeds" >=> POST >=> SubscribeToFeedHandler.subscribeToFeedHandler
        route "/feeds" >=> GET >=> GetSubscriptionsHandler.getSubscriptionsHandler
        route "/subscription/delete" >=> POST >=> DeleteSubscriptionHandler.deleteSubscriptionHandler
        route "/poll" >=> POST >=> FetchEntriesHandler.fetchEntriesHandler ]
