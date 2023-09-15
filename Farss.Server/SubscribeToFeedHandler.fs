module SubscribeToFeedHandler

open Giraffe
open Microsoft.AspNetCore.Http
open FeedReaderAdapter
open Microsoft.Extensions.DependencyInjection
open Persistence
open GiraffeUtils

let previewSubscribeToFeedHandler: HttpHandler =
    fun next (ctx: HttpContext) ->
        let adapter = ctx.RequestServices.GetService<FeedReaderAdapter>()

        ctx.BindJsonAsync<Dto.PreviewSubscribeToFeedQueryDto>()
        |> Task.bindR (SubscribeToFeedWorkflow.previewSubscribeToFeed adapter)
        |> Task.bindR(fun x -> convertToJsonResultHandler x next ctx)
            

let subscribeToFeedHandler : HttpHandler =
    fun next (ctx : HttpContext) ->
        let adapter = ctx.RequestServices.GetService<FeedReaderAdapter>()
        let repository = ctx.RequestServices.GetService<SubscriptionRepository>()
        
        ctx.BindJsonAsync<Dto.SubscribeToFeedDto> ()
        |> Task.bindR (SubscribeToFeedWorkflow.subscribeToFeed adapter repository)
        |> Task.bindR (fun x -> convertToJsonResultHandler x next ctx)
