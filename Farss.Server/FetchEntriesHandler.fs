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
            return! Successful.NO_CONTENT next ctx
        }        
       