module GetSubscriptionsHandler

open Falco.Core
open FalcoUtils
open Persistence
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection

let getSubscriptionsHandler: HttpHandler = 
    fun (ctx: HttpContext) ->
        let repository = ctx.RequestServices.GetService<SubscriptionRepository>()

        let dtos = 
            repository.getAll() 
            |> List.map Dto.SubscriptionDto.toDto
            |> Array.ofList
        
        Response.ofJson dtos ctx
