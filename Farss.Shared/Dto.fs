module Dto

open System

type PreviewSubscribeToFeedQueryDto = 
    {
        Url: string
    }

type  FeedError =
    | FetchError of Exception
    | ParseError of Exception

type PreviewSubscribeToFeedResponseDto =
    {
        Title: string
        Url: string
        Type: FeedType    
    }
            
and FeedType =
    | Atom
    | Rss

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
        FeedId: Guid
        Title: string
        IsRead: bool
        PublishedAt: DateTimeOffset
        Content: string
        Summary: string option
        Link: string
    }

type DeleteSubscriptionDto = { Id: Guid option }

type SetArticleReadStatusDto = { ArticleId: Guid option; SetIsReadTo: bool option }
