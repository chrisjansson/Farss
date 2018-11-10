module DeleteSubscriptionHandler

open Giraffe
open Persistence
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Domain

let deleteSubscriptionHandler: HttpHandler = 
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            //todo: bind incoming value types
            let repository = ctx.GetService<FeedRepository>()
            let! cmd = ctx.BindJsonAsync<DeleteSubscriptionCommand>()

            repository.delete cmd.Id

            return! Successful.NO_CONTENT next ctx
        }