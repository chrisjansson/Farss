module Dto

open System

type SubscriptionDto =
    {
        Id: Guid
        Url: string
    }

module SubscriptionDto = 
    let toDto (feed: Domain.Feed): SubscriptionDto = { Id = feed.Id; Url = feed.Url }