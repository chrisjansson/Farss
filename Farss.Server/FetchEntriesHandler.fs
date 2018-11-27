module FetchEntriesHandler

open System
open Domain
open Persistence
open FeedReaderAdapter
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive

let fetchEntriesHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let adapter = ctx.GetService<FeedReaderAdapter>()
            let subscriptionRepository = ctx.GetService<SubscriptionRepository>()
            let articleRepository = ctx.GetService<ArticleRepository>()

            let subscriptions = subscriptionRepository.getAll()

            for s in subscriptions do
                let! getFeedResult = adapter.getFromUrl(s.Url)
                match getFeedResult with
                | Ok feed ->
                    for item in feed.Items do
                        let article = { Domain.Article.Title = item.Title; Id = Guid.NewGuid(); Guid = "" }
                        articleRepository.save(article)
                | Error e -> ()

            return! Successful.NO_CONTENT next ctx
        }        
       