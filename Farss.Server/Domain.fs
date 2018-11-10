module Domain

open System

type FeedId = Guid

type Feed =
    {
        Id: FeedId
        Url: string
    }

type DeleteSubscriptionCommand = { Id: FeedId }