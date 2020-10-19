module DeleteSubscriptionHandler

open Falco
open FalcoUtils
open Persistence
open Microsoft.AspNetCore.Http
open Dto
open GiraffeUtils

let deleteSubscriptionHandler: HttpHandler = 
    fun (ctx: HttpContext) ->
        let repository = ctx.GetService<SubscriptionRepository>()
        
        let deleteSubscription = DeleteSubscriptionWorkflow.deleteSubscription repository
        
        Request.tryBindJsonOptions<DeleteSubscriptionDto> ctx
        |> TaskResult.bind deleteSubscription
        |> Task.bind (fun x -> convertToJsonResultHandler x ctx)
