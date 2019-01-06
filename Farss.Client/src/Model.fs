﻿module Model

open System

type Model =
    | Loading
    | Loaded of Loaded
and Loaded =
    {
        Articles: Dto.ArticleDto list
        Subscriptions: Dto.SubscriptionDto list
        SubInput: string
    }

type Msg = 
    | Loaded of Dto.SubscriptionDto list * Dto.ArticleDto list
    | LoadingError of string
        //todo: change for domain alias
    | DeleteSubscription of Guid
    | SubscriptionDeleted
    | SubscriptionDeleteFailed of exn
    | Poll
    | Reload
    | OnChangeSub of string
    | AddSubscription
    | SubscriptionSucceeded
    | SubscriptionFailed of exn