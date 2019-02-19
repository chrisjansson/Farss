namespace Dto

module SubscriptionDto = 

    let toDto (subscription: Domain.Subscription): SubscriptionDto = { Id = subscription.Id; Url = subscription.Url; Title = subscription.Title }


module DeleteSubscriptionDto =
    open Domain
    open DtoValidation
    open Reflection

    let toCommand (dto: DeleteSubscriptionDto): Result<DeleteSubscriptionCommand, string> = result {
        let! id = Option.value (nameof <@ dto.Id @>) dto.Id
        return { DeleteSubscriptionCommand.Id = id }
    }

module SetArticleReadStatusDto =
    open Domain
    open DtoValidation
    open Reflection

    let toCommand (dto: SetArticleReadStatusDto): Result<SetArticleReadStatusCommand, string> = result {
        let! articleId = dto.ArticleId |> Option.value (nameof <@ dto.ArticleId @>)
        let! setIsReadTo = dto.SetIsReadTo |> Option.value (nameof <@ dto.SetIsReadTo @>)
            
        return { ArticleId = articleId; SetIsReadTo = setIsReadTo }
    }

module Articles =
    open Domain