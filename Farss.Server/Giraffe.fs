module Farss.Giraffe

open System.IO
open Giraffe
open Giraffe.EndpointRouting
open Microsoft.AspNetCore.Http

let private requireAuthentication (authenticationScheme: string) = requiresAuthentication (RequestErrors.unauthorized authenticationScheme "Realm" (text "auth failed"))

let private data = if File.Exists "commit.txt" then File.ReadAllText "commit.txt" |> Some else None

let private echo: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        json { Dto.StartupInformationDto.BaseUrl = "/"; CommitInformation = data } next ctx
        
let private conditionalSubDir (subDir: string option) endpoint =
    match subDir with
    | Some d -> Routers.subRoute d endpoint
    | None -> Routers.subRoute "/" endpoint
    

let endpoints (subdir: string option) (authenticationScheme: string) =
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
    |> Routers.subRoute (Option.defaultValue "/" subdir)
    |> applyBefore (requireAuthentication authenticationScheme)
    |> List.singleton
