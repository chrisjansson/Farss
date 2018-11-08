module Farss.Giraffe

open Giraffe
open FeedReaderAdapter
open Persistence
open Microsoft.AspNetCore.Http
open Domain
open FSharp.Control.Tasks.V2.ContextInsensitive
open SubscribeToFeedWorkflow

let convertToHandler (result: Result<Unit, WorkflowError>) =
    match result with
    | Ok _ -> Successful.NO_CONTENT
    //todo: what to do with the exception?
    | Error (BadRequest (message, _)) -> RequestErrors.BAD_REQUEST message

let someHttpHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let adapter = ctx.GetService<FeedReaderAdapter>()
            let repository = ctx.GetService<FeedRepository>()
            let! dto = ctx.BindJsonAsync<SubscribeToFeedCommand>()            

            let! result = SubscribeToFeedWorkflow.subscribeToFeed adapter repository dto 
                        
            return! (result |> convertToHandler) next ctx
        }

let getFeedsHandler: HttpHandler = 
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            //let adapter = ctx.GetService<FeedReaderAdapter>()
            //let repository = ctx.GetService<FeedRepository>()
            //let! dto = ctx.BindJsonAsync<SubscribeToFeedCommand>()    

            return! Successful.NO_CONTENT next ctx
        }

let createWebApp () =
    choose [
        route "/ping"   >=> text "pong"
        route "/"       >=> htmlFile "/pages/index.html"
        route "/feeds" >=> POST >=> someHttpHandler
        route "/feeds" >=> GET >=> getFeedsHandler ]
