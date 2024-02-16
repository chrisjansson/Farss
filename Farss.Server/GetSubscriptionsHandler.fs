module GetSubscriptionsHandler

open ORMappingConfiguration
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open System.Linq
open Dto
open Giraffe

let getSubscriptionsHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let context = ctx.RequestServices.GetService<ReaderContext>()

        let subscriptions =
            context
                .Subscriptions
                .Select(fun x ->
                    {| ATitle = x.Title
                       BUnread = x.Articles.Where(fun a -> not a.IsRead).Count()
                       CId = x.Id
                       DUrl = x.Url
                       EIcon = x.Icon |})
                .ToList()
                
        let dtos =
            subscriptions
            |> Seq.map (fun x -> { Dto.SubscriptionDto.Id = x.CId; Title = x.ATitle; Unread = x.BUnread; Url = x.DUrl; Icon = Option.ofNullable x.EIcon })
            |> Array.ofSeq

        Successful.ok (json dtos) next ctx
