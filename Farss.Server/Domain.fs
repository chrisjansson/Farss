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
        Guid: string
        Subscription: SubscriptionId
        Content: string
        IsRead: bool
    }
and ArticleId = Guid

type DeleteSubscriptionCommand = { Id: SubscriptionId }
type SetArticleReadStatusCommand = { ArticleId: ArticleId; SetIsReadTo: bool }