module SubscribeToFeedHandler

open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open SubscribeToFeedWorkflow
open FeedReaderAdapter
open Persistence
open GiraffeUtils

let previewSubscribeToFeedHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let adapter = ctx.GetService<FeedReaderAdapter>()
            let! dto = ctx.BindJsonAsync<Dto.PreviewSubscribeToFeedQueryDto>()            

            let! result = SubscribeToFeedWorkflow.previewSubscribeToFeed adapter dto 
                            
            return! (result |> convertToJsonResultHandler) next ctx
        }

let subscribeToFeedHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let adapter = ctx.GetService<FeedReaderAdapter>()
            let repository = ctx.GetService<SubscriptionRepository>()
            let! dto = ctx.BindJsonAsync<Dto.SubscribeToFeedDto>()            

            let! result = SubscribeToFeedWorkflow.subscribeToFeed adapter repository dto 
                        
            return! (result |> convertToJsonResultHandler) next ctx
        }