module Dto

open System

type SubscriptionDto =
    {
        Id: Guid
        Url: string
    }

type ArticleDto =
    {
        Title: string
        IsRead: bool
        PublishedAt: DateTimeOffset
    }

module SubscriptionDto = 
    let toDto (feed: Domain.Subscription): SubscriptionDto = { Id = feed.Id; Url = feed.Url }

type DeleteSubscriptionDto = { Id: Nullable<Guid> }

type SetArticleReadStatusDto = { ArticleId: Nullable<Guid>; SetIsReadTo: Nullable<bool> }

module DeleteSubscriptionDto =
    open Domain
    open DtoValidation
    open Reflection

    let toCommand (dto: DeleteSubscriptionDto): Result<DeleteSubscriptionCommand, string> = result {
        let! id = Nullable.value (nameof <@ dto.Id @>) dto.Id
        return { DeleteSubscriptionCommand.Id = id }
    }

module SetArticleReadStatusDto =
    open Domain
    open DtoValidation
    open Reflection

    let toCommand (dto: SetArticleReadStatusDto): Result<SetArticleReadStatusCommand, string> = result {
        let! articleId = dto.ArticleId |> Nullable.value (nameof <@ dto.ArticleId @>)
        let! setIsReadTo = dto.SetIsReadTo |> Nullable.value (nameof <@ dto.SetIsReadTo @>)
            
        return { ArticleId = articleId; SetIsReadTo = setIsReadTo }
    }

module Articles =
    open Domain