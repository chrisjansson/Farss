module Domain

open System

type Subscription =
    {
        Id: SubscriptionId
        Url: string
    }
and SubscriptionId = Guid

type Article =
    {
        Id: ArticleId
        Title: string
        Guid: ArticleGuid
        Subscription: SubscriptionId
        Content: string
        IsRead: bool
        PublishedAt: DateTimeOffset
    }
and ArticleId = Guid
and ArticleGuid = string //TODO: wrap in DU?

module ArticleGuid =
    let create (str: string) = 
        if String.IsNullOrWhiteSpace(str) then
            Error "Article guid cannot be null or empty"
        else
            Ok str 

type DeleteSubscriptionCommand = { Id: SubscriptionId }
type SetArticleReadStatusCommand = { ArticleId: ArticleId; SetIsReadTo: bool }