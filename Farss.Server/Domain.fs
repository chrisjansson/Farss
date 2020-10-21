module Domain

open System

type Subscription =
    {
        Id: SubscriptionId
        Url: string
        Title: SubscriptionTitle
        Icon: Guid option
    }
and SubscriptionId = Guid
and SubscriptionTitle = string

module SubscriptionTitle =
    let create v =
        if String.IsNullOrWhiteSpace(v) then //TODO: Extract validation helpers for common rules
            Error "Article guid cannot be null or empty"
        else
            Ok v

module Subscription =
    let create url title: Subscription =
        {
            Id = Guid.NewGuid()
            Url = url
            Title = title
            Icon = None
        }

type Article =
    {
        Id: ArticleId
        Title: string
        Guid: ArticleGuid
        Subscription: SubscriptionId
        Summary: string option
        Content: string
        IsRead: bool
        Timestamp: ArticleTimestamp
        Link: ArticleLink
    }
and ArticleId = Guid
and ArticleGuid = string //TODO: wrap in DU?
and ArticleTimestamp = DateTimeOffset //TODO: wrap in DU?
and ArticleLink = string

type File =
    {
        Id: Guid
        FileName: string
        FileOwner: FileOwner
        Data: byte[]
    }
and FileOwner =
    | Feed = 1

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

module ArticleLink = 
    let create (str: string option) =
        match str with
        | Some dto -> Ok dto
        | None -> Error "Link is required"

module Article =
    let create title guid subscription content summary timestamp link =
        { 
            Id = Guid.NewGuid(); 
            Title = title; 
            Guid = guid; 
            Subscription = subscription; 
            Content = content; 
            Timestamp = timestamp; 
            IsRead = false 
            Link = link
            Summary = summary
        }

type DeleteSubscriptionCommand = { Id: SubscriptionId }
type SetArticleReadStatusCommand = { ArticleId: ArticleId; SetIsReadTo: bool }