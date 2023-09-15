module DeleteSubscriptionHandler

open Microsoft.AspNetCore.Http
open Dto
open GiraffeUtils
open Giraffe
open Persistence

let deleteSubscriptionHandler: HttpHandler = 
    fun next (ctx: HttpContext) ->
        let repository = ctx.GetService<SubscriptionRepository>()
        
        let deleteSubscription = DeleteSubscriptionWorkflow.deleteSubscription repository
        
        ctx.BindJsonAsync<DeleteSubscriptionDto> ()
        |> Task.map deleteSubscription
        |> Task.bindR (fun x -> convertToJsonResultHandler x next ctx)
