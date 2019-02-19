module Model

open System

type Model =
    | Loading
    | Loaded of Loaded
and Loaded =
    {
        Articles: Dto.ArticleDto list
        Subscriptions: Dto.SubscriptionDto list
        AddSubscriptionModel: AddSubscriptionModel.Model option
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
    | OpenAddSubscription
    | AddSubscriptionMsg of AddSubscriptionModel.Message