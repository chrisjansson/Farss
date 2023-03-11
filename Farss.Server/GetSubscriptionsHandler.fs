module GetSubscriptionsHandler

open Falco.Core
open FalcoUtils
open Persistence
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open System.Linq
open Dto

let getSubscriptionsHandler: HttpHandler =
    fun (ctx: HttpContext) ->
        let context = ctx.RequestServices.GetService<ReaderContext>()

        let subscriptions =
            context
                .Subscriptions
                .Select(fun x ->
                    {| ATitle = x.Title
                       BUnread = x.Articles.Where(fun a -> not a.IsRead).Count()
                       CId = x.Id
                       DUrl = x.Url |})
                .ToList()
                
        let dtos =
            subscriptions
            |> Seq.map (fun x -> { Dto.SubscriptionDto.Id = x.CId; Title = x.ATitle; Unread = x.BUnread; Url = x.DUrl })
            |> Array.ofSeq

        Response.ofJson dtos ctx
