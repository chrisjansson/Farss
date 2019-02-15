module Dto

open System

type PreviewSubscribeToFeedDto = 
    {
        Url: string
    }

type PreviewSubscribeToFeedResponseDto =
    {
        Title: string
    }

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

type DeleteSubscriptionDto = { Id: Guid option }

type SetArticleReadStatusDto = { ArticleId: Guid option; SetIsReadTo: bool option }
