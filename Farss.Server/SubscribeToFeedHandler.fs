module SubscribeToFeedHandler

open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open SubscribeToFeedWorkflow
open FeedReaderAdapter
open Persistence
open GiraffeUtils

let subscribeToFeedHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let adapter = ctx.GetService<FeedReaderAdapter>()
            let repository = ctx.GetService<FeedRepository>()
            let! dto = ctx.BindJsonAsync<SubscribeToFeedCommand>()            

            let! result = SubscribeToFeedWorkflow.subscribeToFeed adapter repository dto 
                        
            return! (result |> convertToHandler) next ctx
        }