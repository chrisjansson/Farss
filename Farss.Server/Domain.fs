module Domain

open System

type SubscriptionId = Guid

type Subscription =
    {
        Id: SubscriptionId
        Url: string
    }

type DeleteSubscriptionCommand = { Id: SubscriptionId }