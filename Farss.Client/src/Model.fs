module Model

open System

type Model =
    | Loading
    | Loaded of Dto.SubscriptionDto list * Dto.ArticleDto list

type Msg = 
    | Loaded of Dto.SubscriptionDto list * Dto.ArticleDto list
    | LoadingError of string
        //todo: change for domain alias
    | DeleteSubscription of Guid
    | SubscriptionDeleted
    | SubscriptionDeleteFailed of exn
    | Poll
    | Reload
