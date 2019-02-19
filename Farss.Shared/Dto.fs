module Dto

open System

type PreviewSubscribeToFeedQueryDto = 
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
        Title: string
    }

type SubscriptionDto =
    {
        Id: Guid
        Title: string
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
