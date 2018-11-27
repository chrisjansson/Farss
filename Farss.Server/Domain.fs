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
    }
and ArticleId = Guid

type DeleteSubscriptionCommand = { Id: SubscriptionId }