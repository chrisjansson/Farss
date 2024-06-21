module Dto

open System

type PreviewSubscribeToFeedQueryDto = 
    {
        Url: string
    }

type  FeedError =
    | FetchError of string
    | ParseError of string

type PreviewSubscribeToFeedResponseDto =
    {
        Title: string
        Url: string
        Type: FeedType
        Icon: (string * byte[]) option
        Protocol: Protocol
    }
            
and FeedType =
    | Atom
    | Rss
and Protocol =
    | Http
    | Https


type GetFileDto =
    {
        Id: Guid
    }
and FileDto =
    {
        Id: Guid
        FileName: string
        Data: byte[]
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
        Unread: int
        Icon: Guid option
    }

type ArticleDto =
    {
        Id: Guid
        FeedId: Guid
        Title: string
        IsRead: bool
        PublishedAt: DateTimeOffset
        Content: string
        Summary: string option
        Link: string
    }
    
type GetArticlesQuery =
    {
        Feed: Guid option
        Count: int
    }

type DeleteSubscriptionDto = { Id: Guid option }

type SetArticleReadStatusDto = { ArticleId: Guid option; SetIsReadTo: bool option }

type StartupInformationDto =
    {
        WhoAmI: string
        CommitInformation: string option
    }
