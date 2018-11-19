module DeleteSubscriptionHandler

open Giraffe
open Persistence
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Dto
open GiraffeUtils

let deleteSubscriptionHandler: HttpHandler = 
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let repository = ctx.GetService<SubscriptionRepository>()
            let! cmd = ctx.BindJsonAsync<DeleteSubscriptionDto>()

            let result = 
                DeleteSubscriptionWorkflow.deleteSubscription repository cmd
                |> convertToHandler

            return! result next ctx
        }