namespace Dto

module SubscriptionDto = 

    let toDto (feed: Domain.Subscription): SubscriptionDto = { Id = feed.Id; Url = feed.Url }


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