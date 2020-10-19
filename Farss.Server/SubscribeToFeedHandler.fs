module SubscribeToFeedHandler

open Falco.Core
open Microsoft.AspNetCore.Http
open FeedReaderAdapter
open Microsoft.Extensions.DependencyInjection
open Persistence
open FalcoUtils
open GiraffeUtils

let previewSubscribeToFeedHandler: HttpHandler =
    fun (ctx: HttpContext) ->
        let adapter = ctx.RequestServices.GetService<FeedReaderAdapter>()

        Request.tryBindJsonOptions<Dto.PreviewSubscribeToFeedQueryDto> ctx
        |> TaskResult.bindTask (SubscribeToFeedWorkflow.previewSubscribeToFeed adapter)
        |> Task.bind (fun x -> convertToJsonResultHandler x ctx)

let subscribeToFeedHandler : HttpHandler =
    fun (ctx : HttpContext) ->
        let adapter = ctx.RequestServices.GetService<FeedReaderAdapter>()
        let repository = ctx.RequestServices.GetService<SubscriptionRepository>()
        
        Request.tryBindJsonOptions<Dto.SubscribeToFeedDto> ctx
        |> TaskResult.bindTask (SubscribeToFeedWorkflow.subscribeToFeed adapter repository)
        |> Task.bind (fun x -> convertToJsonResultHandler x ctx)
