module Dto

open System

type SubscribeToFeedDto = 
    {
        Url: string
    }

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

type DeleteSubscriptionDto = { Id: Nullable<Guid> }

type SetArticleReadStatusDto = { ArticleId: Nullable<Guid>; SetIsReadTo: Nullable<bool> }
