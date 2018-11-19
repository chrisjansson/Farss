module GetSubscriptionsHandler

open Giraffe
open Persistence
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive

let getSubscriptionsHandler: HttpHandler = 
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let repository = ctx.GetService<SubscriptionRepository>()

            let dtos = 
                repository.getAll() 
                |> List.map Dto.SubscriptionDto.toDto
                |> Array.ofList

            return! Successful.ok (json dtos) next ctx
        }