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
        PublishedAt: ArticleTimestamp
    }
and ArticleId = Guid
and ArticleGuid = string //TODO: wrap in DU?
and ArticleTimestamp = DateTimeOffset //TODO: wrap in DU?

module ArticleGuid =
    let create (str: string) = 
        if String.IsNullOrWhiteSpace(str) then
            Error "Article guid cannot be null or empty"
        else
            Ok str 

module ArticleTimestamp =
    let create v =
        match v with
        | Some dto -> Ok dto
        | None -> Error "Timestamp is required"

module Article =
    let create title guid subscription content timestamp =
        { 
            Id = Guid.NewGuid(); 
            Title = title; 
            Guid = guid; 
            Subscription = subscription; 
            Content = content; 
            PublishedAt = timestamp; 
            IsRead = false 
        }

type DeleteSubscriptionCommand = { Id: SubscriptionId }
type SetArticleReadStatusCommand = { ArticleId: ArticleId; SetIsReadTo: bool }