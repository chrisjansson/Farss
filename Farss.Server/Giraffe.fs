module Farss.Giraffe

open Giraffe
open FeedReaderAdapter
open Persistence
open Microsoft.AspNetCore.Http
open Domain
open FSharp.Control.Tasks.V2.ContextInsensitive

let someHttpHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let adapter = ctx.GetService<FeedReaderAdapter>()
            let repository = ctx.GetService<FeedRepository>()
            let! dto = ctx.BindJsonAsync<SubscribeToFeedDto>()            

            let feed: Feed = { Url = "Weeee" }
            repository.save feed

            return! Successful.NO_CONTENT next ctx
        }

let createWebApp () =
    choose [
        route "/ping"   >=> text "pong"
        route "/"       >=> htmlFile "/pages/index.html"
        route "/feeds"   >=> someHttpHandler ]
