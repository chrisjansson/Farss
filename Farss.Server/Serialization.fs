namespace Dto

module Option =
    open System
    let value (name: string) (v: Option<'a>) =
        match v with
        | Some v -> Ok v
        | None -> Error name


    let tap f v =
        match v with
        | Some x -> f x; v
        | None -> v


module SubscriptionDto = 

    let toDto (subscription: Domain.Subscription) (unread: int) : SubscriptionDto =
        {
            Id = subscription.Id
            Url = subscription.Url
            Title = subscription.Title
            Unread = unread
            Icon = subscription.Icon 
        }


module DeleteSubscriptionDto =
    open Domain
    open Reflection

    let toCommand (dto: DeleteSubscriptionDto): Result<DeleteSubscriptionCommand, string> = result {
        let! id = Option.value (nameof <@ dto.Id @>) dto.Id
        return { DeleteSubscriptionCommand.Id = id }
    }

module SetArticleReadStatusDto =
    open Domain
    open Reflection

    let toCommand (dto: SetArticleReadStatusDto): Result<SetArticleReadStatusCommand, string> = result {
        let! articleId = dto.ArticleId |> Option.value (nameof <@ dto.ArticleId @>)
        let! setIsReadTo = dto.SetIsReadTo |> Option.value (nameof <@ dto.SetIsReadTo @>)
            
        return { ArticleId = articleId; SetIsReadTo = setIsReadTo }
    }
